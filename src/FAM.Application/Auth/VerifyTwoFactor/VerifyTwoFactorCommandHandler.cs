using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace FAM.Application.Auth.VerifyTwoFactor;

/// <summary>
/// Handler for verifying 2FA code
/// </summary>
public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, VerifyTwoFactorResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IJwtService _jwtService;
    private readonly ISigningKeyService _signingKeyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        IJwtService jwtService,
        ISigningKeyService signingKeyService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyTwoFactorCommandHandler> _logger)
    {
        _userRepository = userRepository;
        _userDeviceRepository = userDeviceRepository;
        _jwtService = jwtService;
        _signingKeyService = signingKeyService;
        _unitOfWork = unitOfWork;
        this._logger = _logger;
    }

    public async Task<VerifyTwoFactorResponse> Handle(VerifyTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        // Extract user ID from session token (token already validated by JWT middleware)
        var userId = _jwtService.GetUserIdFromToken(request.TwoFactorSessionToken);
        if (userId == 0)
            throw new UnauthorizedAccessException("Invalid or expired two-factor session token");

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

        // Generate tokens using RSA
        var activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);
        var roles = new List<string>(); // TODO: Load user roles from UserNodeRoles
        var accessToken = _jwtService.GenerateAccessTokenWithRsa(
            user.Id,
            user.Username.Value,
            user.Email.Value,
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);
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