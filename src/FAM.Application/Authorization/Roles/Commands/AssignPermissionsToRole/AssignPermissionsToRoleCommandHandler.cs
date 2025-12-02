using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common;
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
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.RoleId} not found");

        var existingRolePermissions =
            await _unitOfWork.RolePermissions.GetByRoleIdAsync(request.RoleId, cancellationToken);
        var existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        var newPermissionIds = request.PermissionIds.Where(id => !existingPermissionIds.Contains(id)).ToList();

        if (!newPermissionIds.Any())
            return true;

        foreach (var permissionId in newPermissionIds)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
                throw new NotFoundException(ErrorCodes.PERMISSION_NOT_FOUND,
                    $"Permission with ID {permissionId} not found");
        }

        foreach (var permissionId in newPermissionIds)
        {
            var rolePermission = RolePermission.Create(request.RoleId, permissionId, request.AssignedById);
            await _unitOfWork.RolePermissions.AddAsync(rolePermission, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}