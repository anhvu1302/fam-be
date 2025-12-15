using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.VerifyEmailOtp;

/// <summary>
/// Handler for verifying email OTP during login flow
/// </summary>
public class VerifyEmailOtpLoginCommandHandler
    : IRequestHandler<VerifyEmailOtpLoginCommand, VerifyEmailOtpLoginResponse>
{
    private readonly IOtpService _otpService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISigningKeyService _signingKeyService;
    private readonly IJwtService _jwtService;
    private readonly ITwoFactorSessionService _twoFactorSessionService;
    private readonly ILogger<VerifyEmailOtpLoginCommandHandler> _logger;

    public VerifyEmailOtpLoginCommandHandler(
        IOtpService otpService,
        IUnitOfWork unitOfWork,
        ISigningKeyService signingKeyService,
        IJwtService jwtService,
        ITwoFactorSessionService twoFactorSessionService,
        ILogger<VerifyEmailOtpLoginCommandHandler> logger)
    {
        _otpService = otpService;
        _unitOfWork = unitOfWork;
        _signingKeyService = signingKeyService;
        _jwtService = jwtService;
        _twoFactorSessionService = twoFactorSessionService;
        _logger = logger;
    }

    public async Task<VerifyEmailOtpLoginResponse> Handle(
        VerifyEmailOtpLoginCommand request,
        CancellationToken cancellationToken)
    {

        // Find user to get their ID for OTP verification
        User? user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_CREDENTIALS, "User not found");

        // Verify OTP using the OTP service
        // The session token would need to be stored somewhere or we can use email as identifier
        // For email verification in login flow, we use email as the session identifier
        var isValidOtp = await _otpService.VerifyOtpAsync(user.Id, request.Email, request.EmailOtp, cancellationToken);

        if (!isValidOtp)
        {
            _logger.LogWarning("Invalid or expired OTP for user {UserId} ({Email})", user.Id, request.Email);
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_2FA_CODE, "Invalid or expired OTP code");
        }

        // OTP verified, remove from cache
        await _otpService.RemoveOtpAsync(user.Id, request.Email, cancellationToken);

        // Mark email as verified if not already
        if (!user.IsEmailVerified)
        {
            // Set IsEmailVerified property through reflection since the property might not have a setter
            typeof(User).GetProperty("IsEmailVerified")?.SetValue(user, true);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Check if 2FA is enabled
        if (user.TwoFactorEnabled)
        {
            var twoFactorSessionToken = await GenerateTwoFactorSessionTokenAsync(user.Id, cancellationToken);

            return new VerifyEmailOtpLoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorSessionToken = twoFactorSessionToken,
                User = MapToUserInfoDto(user)
            };
        }

        // Generate tokens
        SigningKey activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);
        var roles = new List<string>(); // User roles can be loaded separately if needed

        var accessToken = _jwtService.GenerateAccessTokenWithRsa(
            user.Id,
            user.Username.Value,
            user.Email.Value,
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);

        // Calculate expiration times from config
        DateTime accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.AccessTokenExpiryMinutes);
        var refreshToken = _jwtService.GenerateRefreshToken();
        DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpiryDays);

        return new VerifyEmailOtpLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer",
            User = MapToUserInfoDto(user),
            RequiresTwoFactor = false
        };
    }

    private async Task<string> GenerateTwoFactorSessionTokenAsync(long userId, CancellationToken cancellationToken)
    {
        // Use simple session token instead of JWT (no need for cryptographic signing for temporary tokens)
        return await _twoFactorSessionService.CreateSessionAsync(userId, expirationMinutes: 10, cancellationToken);
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
}
