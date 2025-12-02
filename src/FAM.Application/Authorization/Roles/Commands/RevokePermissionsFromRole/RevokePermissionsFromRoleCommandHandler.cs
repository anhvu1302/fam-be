using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.RevokePermissionsFromRole;

public sealed class RevokePermissionsFromRoleCommandHandler : IRequestHandler<RevokePermissionsFromRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokePermissionsFromRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RevokePermissionsFromRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.RoleId} not found");

        var existingRolePermissions =
            await _unitOfWork.RolePermissions.GetByRoleIdAsync(request.RoleId, cancellationToken);

        foreach (var permissionId in request.PermissionIds)
        {
            var rolePermission = existingRolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
            if (rolePermission != null) _unitOfWork.RolePermissions.Delete(rolePermission);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}