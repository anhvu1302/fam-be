using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

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
    private readonly ITwoFactorSessionService _twoFactorSessionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        IJwtService jwtService,
        ISigningKeyService signingKeyService,
        ITwoFactorSessionService twoFactorSessionService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyTwoFactorCommandHandler> _logger)
    {
        _userRepository = userRepository;
        _userDeviceRepository = userDeviceRepository;
        _jwtService = jwtService;
        _signingKeyService = signingKeyService;
        _twoFactorSessionService = twoFactorSessionService;
        _unitOfWork = unitOfWork;
        this._logger = _logger;
    }

    public async Task<VerifyTwoFactorResponse> Handle(VerifyTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        // Validate 2FA session token
        long userId =
            await _twoFactorSessionService.ValidateAndGetUserIdAsync(request.TwoFactorSessionToken, cancellationToken);
        if (userId == 0)
        {
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_TOKEN);
        }

        User? user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.USER_NOT_FOUND);
        }

        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            throw new UnauthorizedException(ErrorCodes.AUTH_2FA_REQUIRED);
        }

        if (!VerifyTwoFactorCode(user.TwoFactorSecret, request.TwoFactorCode))
        {
            _logger.LogWarning("Invalid 2FA code provided for user {UserId}", userId);
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_2FA_CODE);
        }

        UserDevice device = user.GetOrCreateDevice(
            request.DeviceId,
            request.DeviceName ?? "Unknown Device",
            request.DeviceType ?? "browser",
            request.UserAgent,
            request.IpAddress,
            request.Location
        );

        SigningKey activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);
        List<string> roles = new();
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
        DateTime refreshTokenExpiresAt =
            DateTime.UtcNow.AddDays(request.RememberMe ? _jwtService.RefreshTokenExpiryDays : 7);

        device.UpdateTokens(refreshToken, refreshTokenExpiresAt, accessTokenJti ?? string.Empty, request.IpAddress);

        user.RecordLogin(request.IpAddress);

        UserDevice? existingDeviceInDb =
            await _unitOfWork.UserDevices.GetByDeviceIdAsync(request.DeviceId, cancellationToken);

        if (existingDeviceInDb != null)
        {
            existingDeviceInDb.UpdateTokens(refreshToken, refreshTokenExpiresAt, accessTokenJti ?? string.Empty,
                request.IpAddress);
            _unitOfWork.UserDevices.Update(existingDeviceInDb);
        }
        else
        {
            await _unitOfWork.UserDevices.AddAsync(device, cancellationToken);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyTwoFactorResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer",
            User = new UserInfoDto
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
            }
        };
    }

    private bool VerifyTwoFactorCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
        {
            return false;
        }

        try
        {
            // Decode base32 secret
            byte[]? secretBytes = Base32Encoding.ToBytes(secret);
            Totp totp = new(secretBytes);

            // Verify code with time window tolerance (1 step = 30 seconds)
            // This allows for slight clock skew between server and client
            VerificationWindow verificationWindow = new(1, 1);

            return totp.VerifyTotp(code, out _, verificationWindow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            return false;
        }
    }
}
