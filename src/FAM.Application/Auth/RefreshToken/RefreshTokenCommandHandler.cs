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
        // Find device by refresh token
        var device = await _unitOfWork.UserDevices.FindByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (device == null) throw new UnauthorizedAccessException("Invalid refresh token");

        // Check if refresh token is still valid
        if (!device.IsRefreshTokenValid()) throw new UnauthorizedAccessException("Refresh token has expired");

        // Check if device is active
        if (!device.IsActive) throw new UnauthorizedAccessException("Device is inactive");

        // Get the user
        var user = await _unitOfWork.Users.GetByIdAsync(device.UserId, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("User not found");

        // Check if user is active and not locked
        if (!user.IsActive) throw new UnauthorizedAccessException("Account is inactive");

        if (user.IsLockedOut()) throw new UnauthorizedAccessException("Account is locked");

        // Generate new access token using RSA
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

        // Optionally generate new refresh token (rotate refresh token for better security)
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);

        // Update device with new refresh token
        device.UpdateRefreshToken(newRefreshToken, refreshTokenExpiresAt, request.IpAddress, request.Location);
        _unitOfWork.UserDevices.Update(device);

        // Update last activity
        user.RecordLogin(request.IpAddress);
        _unitOfWork.Users.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600, // 1 hour
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
}