using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
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
    private readonly ITwoFactorSessionService _twoFactorSessionService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ISigningKeyService signingKeyService,
        ITwoFactorSessionService twoFactorSessionService,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _signingKeyService = signingKeyService;
        _twoFactorSessionService = twoFactorSessionService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _unitOfWork.Users.FindByIdentityAsync(request.Identity, cancellationToken);

        // Return generic error for both "user not found" and "wrong password" (security: prevent username enumeration)
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_CREDENTIALS,
                ErrorMessages.GetMessage(ErrorCodes.AUTH_INVALID_CREDENTIALS));
        }

        if (user.IsLockedOut())
        {
            throw new UnauthorizedException(ErrorCodes.AUTH_ACCOUNT_LOCKED,
                ErrorMessages.GetMessage(ErrorCodes.AUTH_ACCOUNT_LOCKED));
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException(ErrorCodes.AUTH_ACCOUNT_INACTIVE,
                ErrorMessages.GetMessage(ErrorCodes.AUTH_ACCOUNT_INACTIVE));
        }

        bool isPasswordValid = user.Password.Verify(request.Password);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return same generic error as user not found (security: prevent password enumeration)
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_CREDENTIALS,
                ErrorMessages.GetMessage(ErrorCodes.AUTH_INVALID_CREDENTIALS));
        }

        if (!user.IsEmailVerified)
        {
            // Generate OTP using OtpService (which handles storage and rate limiting)
            try
            {
                string otp = await _otpService.GenerateOtpAsync(
                    user.Id,
                    user.Email,
                    10,
                    cancellationToken);

                // Send OTP via email
                await _emailService.SendOtpEmailAsync(
                    user.Email,
                    otp,
                    user.Username,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification OTP to {Email}", user.Email);
                throw;
            }

            return new LoginResponse
            {
                RequiresEmailVerification = true,
                MaskedEmail = user.Email,
                User = new UserInfoDto()
            };
        }

        if (user.TwoFactorEnabled)
        {
            string twoFactorSessionToken = await GenerateTwoFactorSessionTokenAsync(user.Id, cancellationToken);

            return new LoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorSessionToken = twoFactorSessionToken,
                User = MapToUserInfoDto(user)
            };
        }

        SigningKey activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);

        // TODO: Load user roles from UserNodeRoles properly
        List<string> roles = new();
        if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            roles.Add("Admin");
            roles.Add("SuperAdmin");
        }

        string accessToken = _jwtService.GenerateAccessTokenWithRsa(
            user.Id,
            user.Username,
            user.Email,
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);

        string? accessTokenJti = _jwtService.GetJtiFromToken(accessToken);

        // Calculate expiration times from config
        DateTime accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.AccessTokenExpiryMinutes);
        string refreshToken = _jwtService.GenerateRefreshToken();
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
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Avatar = user.Avatar,
            PhoneNumber = user.PhoneNumber,
            PhoneCountryCode = user.PhoneCountryCode,
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
        // Use simple session token instead of JWT (no need for cryptographic signing for temporary tokens)
        return await _twoFactorSessionService.CreateSessionAsync(userId, 10, cancellationToken);
    }
}
