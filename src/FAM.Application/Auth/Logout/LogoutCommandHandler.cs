using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Auth.Logout;

/// <summary>
/// Handler for LogoutCommand - logs out user from a specific device
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Find device by refresh token or device ID
        var device = !string.IsNullOrEmpty(request.RefreshToken)
            ? await _unitOfWork.UserDevices.FindByRefreshTokenAsync(request.RefreshToken, cancellationToken)
            : !string.IsNullOrEmpty(request.DeviceId)
                ? await _unitOfWork.UserDevices.GetByDeviceIdAsync(request.DeviceId, cancellationToken)
                : null;

        if (device == null)
            // Device not found - might already be logged out or invalid token
            return Unit.Value;

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(device.UserId, cancellationToken);

        // Clear refresh token and deactivate device
        device.ClearRefreshToken();
        device.Deactivate();
        _unitOfWork.UserDevices.Update(device);

        // TODO: Raise UserLoggedOutEvent
        // var logoutEvent = new UserLoggedOutEvent(device.UserId, device.DeviceId, request.IpAddress, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}