using System.Reflection;
using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Common;
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
    private readonly ILogger<VerifyEmailOtpLoginCommandHandler> _logger;

    public VerifyEmailOtpLoginCommandHandler(
        IOtpService otpService,
        IUnitOfWork unitOfWork,
        ISigningKeyService signingKeyService,
        IJwtService jwtService,
        ILogger<VerifyEmailOtpLoginCommandHandler> logger)
    {
        _otpService = otpService;
        _unitOfWork = unitOfWork;
        _signingKeyService = signingKeyService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<VerifyEmailOtpLoginResponse> Handle(
        VerifyEmailOtpLoginCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying email OTP for {Email}", request.Email);

        // Find user to get their ID for OTP verification
        var user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);
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
        _logger.LogInformation("Email OTP verified and removed for {Email}", request.Email);

        // Mark email as verified if not already
        if (!user.IsEmailVerified)
        {
            // Set IsEmailVerified property through reflection since the property might not have a setter
            typeof(Domain.Users.User).GetProperty("IsEmailVerified")?.SetValue(user, true);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Email marked as verified for user {UserId}", user.Id);
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
        var activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);
        var roles = new List<string>();  // User roles can be loaded separately if needed

        var accessToken = _jwtService.GenerateAccessTokenWithRsa(
            user.Id,
            user.Username.Value,
            user.Email.Value,
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);

        var refreshToken = _jwtService.GenerateRefreshToken();

        return new VerifyEmailOtpLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour
            TokenType = "Bearer",
            User = MapToUserInfoDto(user),
            RequiresTwoFactor = false
        };
    }

    private async Task<string> GenerateTwoFactorSessionTokenAsync(long userId, CancellationToken cancellationToken)
    {
        var activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);
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

    private static UserInfoDto MapToUserInfoDto(Domain.Users.User user)
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
