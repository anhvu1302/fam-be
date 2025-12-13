using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Logout;

/// <summary>
/// Handler for LogoutCommand - logs out user from a specific device
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenBlacklistService tokenBlacklistService,
        IJwtService jwtService,
        ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenBlacklistService = tokenBlacklistService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Find device by refresh token or device ID
        UserDevice? device = !string.IsNullOrEmpty(request.RefreshToken)
            ? await _unitOfWork.UserDevices.FindByRefreshTokenAsync(request.RefreshToken, cancellationToken)
            : !string.IsNullOrEmpty(request.DeviceId)
                ? await _unitOfWork.UserDevices.GetByDeviceIdAsync(request.DeviceId, cancellationToken)
                : null;

        if (device == null)
        {
            // Device not found - might already be logged out or invalid token
            _logger.LogWarning("Logout attempted but device not found");
            return Unit.Value;
        }

        // Get user
        User? user = await _unitOfWork.Users.GetByIdAsync(device.UserId, cancellationToken);

        // Clear refresh token and deactivate device
        device.ClearRefreshToken();
        device.Deactivate();
        _unitOfWork.UserDevices.Update(device);

        // Blacklist the access token to invalidate it immediately
        if (!string.IsNullOrEmpty(request.AccessToken) && request.AccessTokenExpiration.HasValue)
            try
            {
                await _tokenBlacklistService.BlacklistTokenAsync(
                    request.AccessToken,
                    request.AccessTokenExpiration.Value,
                    cancellationToken);
                _logger.LogInformation("Access token blacklisted for user {UserId}", device.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to blacklist access token during logout");
                // Continue with logout even if blacklisting fails
            }

        // TODO: Raise UserLoggedOutEvent
        // var logoutEvent = new UserLoggedOutEvent(device.UserId, device.DeviceId, request.IpAddress, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged out from device {DeviceId}", device.UserId, device.DeviceId);

        return Unit.Value;
    }
}
