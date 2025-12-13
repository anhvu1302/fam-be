using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Login;

/// <summary>
/// Handler for LoginCommand - authenticates user and generates tokens
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ISigningKeyService _signingKeyService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ISigningKeyService signingKeyService,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _signingKeyService = signingKeyService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _unitOfWork.Users.FindByIdentityAsync(request.Identity, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username/email or password");

        if (user.IsLockedOut())
        {
            DateTime lockoutEnd = user.LockoutEnd!.Value;
            var minutesRemaining = (int)(lockoutEnd - DateTime.UtcNow).TotalMinutes + 1;
            throw new UnauthorizedAccessException($"Account is locked. Try again in {minutesRemaining} minutes");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive");

        var isPasswordValid = user.Password.Verify(request.Password);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Invalid username/email or password");
        }

        if (!user.IsEmailVerified)
        {
            _logger.LogInformation("Email not verified for user {UserId}, sending verification OTP", user.Id);

            // Generate OTP using OtpService (which handles storage and rate limiting)
            try
            {
                var otp = await _otpService.GenerateOtpAsync(
                    user.Id,
                    user.Email.Value,
                    10,
                    cancellationToken);

                // Send OTP via email
                await _emailService.SendOtpEmailAsync(
                    user.Email.Value,
                    otp,
                    user.Username.Value,
                    cancellationToken);

                _logger.LogInformation("Verification OTP sent to {Email}", user.Email.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification OTP to {Email}", user.Email.Value);
                throw;
            }

            return new LoginResponse
            {
                RequiresEmailVerification = true,
                MaskedEmail = user.Email.Value,
                User = new UserInfoDto()
            };
        }

        if (user.TwoFactorEnabled)
        {
            var twoFactorSessionToken = await GenerateTwoFactorSessionTokenAsync(user.Id, cancellationToken);

            return new LoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorSessionToken = twoFactorSessionToken,
                User = MapToUserInfoDto(user)
            };
        }

        SigningKey activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);

        // TODO: Load user roles from UserNodeRoles properly
        var roles = new List<string>();
        if (user.Username.Value.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            roles.Add("Admin");
            roles.Add("SuperAdmin");
        }

        var accessToken = _jwtService.GenerateAccessTokenWithRsa(
            user.Id,
            user.Username.Value,
            user.Email.Value,
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);

        var accessTokenJti = _jwtService.GetJtiFromToken(accessToken);

        // Calculate expiration times from config
        DateTime accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.AccessTokenExpiryMinutes);
        var refreshToken = _jwtService.GenerateRefreshToken();
        // Use shorter expiry if RememberMe is false (7 days vs configured default)
        DateTime refreshTokenExpiresAt =
            DateTime.UtcNow.AddDays(request.RememberMe ? _jwtService.RefreshTokenExpiryDays : 7);

        UserDevice device = user.GetOrCreateDevice(
            request.DeviceId,
            request.DeviceName ?? "Unknown Device",
            request.DeviceType ?? "browser",
            request.UserAgent,
            request.IpAddress,
            request.Location
        );

        // Update device with refresh token and active access token JTI
        device.UpdateTokens(refreshToken, refreshTokenExpiresAt, accessTokenJti ?? string.Empty, request.IpAddress);

        user.RecordLogin(request.IpAddress);

        UserDevice? existingDevice = await _unitOfWork.UserDevices.GetByIdAsync(device.Id, cancellationToken);
        if (existingDevice != null)
        {
            existingDevice.UpdateTokens(refreshToken, refreshTokenExpiresAt, accessTokenJti ?? string.Empty,
                request.IpAddress, request.Location);
            _unitOfWork.UserDevices.Update(existingDevice);
        }
        else
        {
            await _unitOfWork.UserDevices.AddAsync(device, cancellationToken);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Raise UserLoggedInEvent
        // var loginEvent = new UserLoggedInEvent(user.Id, device.DeviceId, request.IpAddress, DateTime.UtcNow);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer",
            User = MapToUserInfoDto(user),
            DeviceId = device.DeviceId
        };
    }

    private static UserInfoDto MapToUserInfoDto(User user)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username.Value,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Avatar = user.Avatar,
            PhoneNumber = user.PhoneNumber?.Value,
            PhoneCountryCode = user.PhoneNumber?.CountryCode,
            DateOfBirth = user.DateOfBirth,
            Bio = user.Bio,
            IsEmailVerified = user.IsEmailVerified,
            IsTwoFactorEnabled = user.TwoFactorEnabled,
            PreferredLanguage = user.PreferredLanguage,
            TimeZone = user.TimeZone
        };
    }

    private async Task<string> GenerateTwoFactorSessionTokenAsync(long userId, CancellationToken cancellationToken)
    {
        SigningKey activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);

        var roles = new List<string> { "2fa_session" };
        return _jwtService.GenerateAccessTokenWithRsa(
            userId,
            $"2fa_session_{userId}",
            "",
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);
    }
}
