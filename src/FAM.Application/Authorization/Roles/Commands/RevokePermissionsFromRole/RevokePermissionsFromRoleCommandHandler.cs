using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

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
        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.RoleId} not found");
        }

        IEnumerable<RolePermission> existingRolePermissions =
            await _unitOfWork.RolePermissions.GetByRoleIdAsync(request.RoleId, cancellationToken);

        foreach (long permissionId in request.PermissionIds)
        {
            RolePermission? rolePermission =
                existingRolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
            if (rolePermission != null)
            {
                _unitOfWork.RolePermissions.Delete(rolePermission);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
