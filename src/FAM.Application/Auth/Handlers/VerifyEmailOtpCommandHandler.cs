using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Auth.Services;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler để verify email OTP
/// </summary>
public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, VerifyTwoFactorResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IOtpService _otpService;
    private readonly ILogger<VerifyEmailOtpCommandHandler> _logger;

    public VerifyEmailOtpCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IOtpService otpService,
        ILogger<VerifyEmailOtpCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<VerifyTwoFactorResponse> Handle(
        VerifyEmailOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Validate và extract userId từ 2FA session token
        var userId = ValidateAndExtractUserId(request.TwoFactorSessionToken);
        if (userId == 0)
            throw new UnauthorizedAccessException("Invalid or expired 2FA session token");

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        // Verify OTP with session token
        var isOtpValid = await _otpService.VerifyOtpAsync(userId, request.TwoFactorSessionToken, request.EmailOtp,
            cancellationToken);
        if (!isOtpValid)
        {
            _logger.LogWarning("Invalid email OTP provided for user {UserId}", userId);
            throw new UnauthorizedAccessException("Invalid or expired OTP code");
        }

        // Remove OTP sau khi verify thành công
        await _otpService.RemoveOtpAsync(userId, request.TwoFactorSessionToken, cancellationToken);

        // Generate tokens
        var roles = new List<string>(); // TODO: Load user roles from UserNodeRoles
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username.Value, user.Email.Value, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7);

        // Get or create device
        var device = user.GetOrCreateDevice(
            request.DeviceId,
            request.DeviceName ?? "Unknown Device",
            request.DeviceType ?? "browser",
            request.UserAgent,
            request.IpAddress,
            request.Location
        );

        // Update device with refresh token
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);

        // Update last login info
        user.RecordLogin(request.IpAddress);

        // Save changes
        var existingDevice = await _unitOfWork.UserDevices.GetByIdAsync(device.Id, cancellationToken);
        if (existingDevice != null)
        {
            existingDevice.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);
            existingDevice.RecordLogin(request.IpAddress, request.Location);
            _unitOfWork.UserDevices.Update(existingDevice);
        }
        else
        {
            await _unitOfWork.UserDevices.AddAsync(device, cancellationToken);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully completed email OTP verification", userId);

        return new VerifyTwoFactorResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
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

    private long ValidateAndExtractUserId(string token)
    {
        try
        {
            var principal = _jwtService.ValidateTokenAndGetPrincipal(token);
            if (principal == null)
                return 0;

            var userIdClaim = principal.FindFirst("user_id");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                return 0;

            var roleClaim = principal.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "2fa_session")
                return 0;

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate 2FA session token");
            return 0;
        }
    }
}