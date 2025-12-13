using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

using MediatR;

namespace FAM.Application.Auth.ChangePassword;

/// <summary>
/// Handler for ChangePasswordCommand - changes user password
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user
        User? user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null) throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        // Verify current password
        var isCurrentPasswordValid = user.Password.Verify(request.CurrentPassword);

        if (!isCurrentPasswordValid) throw new UnauthorizedAccessException("Current password is incorrect");

        // Validate new password (Password.Create will throw if invalid)
        var newPassword = Password.Create(request.NewPassword);

        // Update password
        user.UpdatePassword(newPassword.Hash, newPassword.Salt);
        _unitOfWork.Users.Update(user);

        // Optionally: Logout all devices except current one for security
        if (request.LogoutAllDevices)
            await _unitOfWork.UserDevices.DeactivateAllUserDevicesAsync(
                request.UserId,
                request.CurrentDeviceId,
                cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
