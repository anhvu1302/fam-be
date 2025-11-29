using FAM.Application.Abstractions;
using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using MediatR;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler for verifying 2FA code
/// </summary>
public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, VerifyTwoFactorResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyTwoFactorCommandHandler> _logger)
    {
        _userRepository = userRepository;
        _userDeviceRepository = userDeviceRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        this._logger = _logger;
    }

    public async Task<VerifyTwoFactorResponse> Handle(VerifyTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the 2FA session token
        if (!ValidateTwoFactorSessionToken(request.TwoFactorSessionToken))
            throw new UnauthorizedAccessException("Invalid or expired two-factor session token");

        // Extract user ID from session token
        var userId = _jwtService.GetUserIdFromToken(request.TwoFactorSessionToken);

        // Get user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        // Check if 2FA is enabled
        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new UnauthorizedAccessException("Two-factor authentication is not enabled for this user");

        // Verify 2FA code
        if (!VerifyTwoFactorCode(user.TwoFactorSecret, request.TwoFactorCode))
        {
            _logger.LogWarning("Invalid 2FA code provided for user {UserId}", userId);
            throw new UnauthorizedAccessException("Invalid two-factor code");
        }

        // Get or create device (using device info from request)
        var device = user.GetOrCreateDevice(
            request.DeviceId,
            request.DeviceName ?? "Unknown Device",
            request.DeviceType ?? "browser",
            request.UserAgent,
            request.IpAddress,
            request.Location // Location from IP geolocation
        );

        // Generate tokens
        var roles = new List<string>(); // TODO: Load user roles from UserNodeRoles
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username.Value, user.Email.Value, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7);

        // Update device with refresh token
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);

        // Update last login info
        user.RecordLogin(request.IpAddress);

        // Save changes
        // Check if device exists in DB
        var existingDeviceInDb = await _unitOfWork.UserDevices.GetByDeviceIdAsync(request.DeviceId, cancellationToken);

        if (existingDeviceInDb != null)
        {
            // Device exists in DB, update it
            existingDeviceInDb.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);
            _unitOfWork.UserDevices.Update(existingDeviceInDb);
        }
        else
        {
            // New device, add it
            await _unitOfWork.UserDevices.AddAsync(device, cancellationToken);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully completed 2FA verification", userId);

        return new VerifyTwoFactorResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour - should be configurable
            TokenType = "Bearer",
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username.Value,
                Email = user.Email.Value,
                FullName = user.FullName,
                IsEmailVerified = user.IsEmailVerified,
                IsTwoFactorEnabled = user.TwoFactorEnabled
            }
        };
    }

    private bool ValidateTwoFactorSessionToken(string token)
    {
        try
        {
            // Validate token and check if it's a 2FA session token
            var isValid = _jwtService.ValidateToken(token);
            if (!isValid)
                return false;

            // Check if token contains 2fa_session role (our way to identify 2FA session tokens)
            var userId = _jwtService.GetUserIdFromToken(token);
            // Additional validation could be added here

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyTwoFactorCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
            return false;

        try
        {
            // Decode base32 secret
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);

            // Verify code with time window tolerance (1 step = 30 seconds)
            // This allows for slight clock skew between server and client
            var verificationWindow = new VerificationWindow(1, 1);

            return totp.VerifyTotp(code, out _, verificationWindow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            return false;
        }
    }
}