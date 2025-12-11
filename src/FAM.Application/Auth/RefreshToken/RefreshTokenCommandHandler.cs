using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Auth.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand - refreshes access token using refresh token
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ISigningKeyService _signingKeyService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService,
        ISigningKeyService signingKeyService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _signingKeyService = signingKeyService;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.UserDevices.FindByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (device == null) throw new UnauthorizedAccessException("Invalid refresh token");

        if (!device.IsRefreshTokenValid()) throw new UnauthorizedAccessException("Refresh token has expired");

        if (!device.IsActive) throw new UnauthorizedAccessException("Device is inactive");

        var user = await _unitOfWork.Users.GetByIdAsync(device.UserId, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive) throw new UnauthorizedAccessException("Account is inactive");

        if (user.IsLockedOut()) throw new UnauthorizedAccessException("Account is locked");

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

        var accessTokenJti = _jwtService.GetJtiFromToken(accessToken);

        // Calculate expiration times from config
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.AccessTokenExpiryMinutes);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpiryDays);

        // Update device tokens directly (device is detached from FindByRefreshTokenAsync)
        device.UpdateTokens(newRefreshToken, refreshTokenExpiresAt, accessTokenJti ?? string.Empty, 
            request.IpAddress, request.Location);
        
        // Explicitly attach and update the detached entity
        _unitOfWork.UserDevices.Update(device);

        user.RecordLogin(request.IpAddress);
        _unitOfWork.Users.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer",
            User = new UserInfoDto
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
            }
        };
    }
}