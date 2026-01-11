using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.AssignPermissionsToRole;

public sealed class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionsToRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.RoleId} not found");
        }

        IEnumerable<RolePermission> existingRolePermissions =
            await _unitOfWork.RolePermissions.GetByRoleIdAsync(request.RoleId, cancellationToken);
        HashSet<long> existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        List<long> newPermissionIds = request.PermissionIds.Where(id => !existingPermissionIds.Contains(id)).ToList();

        if (!newPermissionIds.Any())
        {
            return true;
        }

        foreach (long permissionId in newPermissionIds)
        {
            Permission? permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
            {
                throw new NotFoundException(ErrorCodes.PERMISSION_NOT_FOUND,
                    $"Permission with ID {permissionId} not found");
            }
        }

        foreach (long permissionId in newPermissionIds)
        {
            RolePermission rolePermission = RolePermission.Create(request.RoleId, permissionId, request.AssignedById);
            await _unitOfWork.RolePermissions.AddAsync(rolePermission, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
