using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Auth.Logout;

/// <summary>
/// Handler for LogoutAllDevicesCommand - logs out user from all devices
/// </summary>
public class LogoutAllDevicesCommandHandler : IRequestHandler<LogoutAllDevicesCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutAllDevicesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(LogoutAllDevicesCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null) throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        // Deactivate all user devices (except current device if specified)
        await _unitOfWork.UserDevices.DeactivateAllUserDevicesAsync(
            request.UserId,
            request.ExceptDeviceId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}