using FAM.Application.Authorization.Permissions.Shared;
using FAM.Application.Authorization.Roles.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;

using MediatR;

namespace FAM.Application.Authorization.Roles.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleWithPermissionsDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleWithPermissionsDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
            return null;

        IEnumerable<RolePermission> rolePermissions =
            await _unitOfWork.RolePermissions.GetByRoleIdAsync(request.Id, cancellationToken);
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        var permissions = new List<PermissionDto>();
        foreach (var permissionId in permissionIds)
        {
            Permission? permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken);
            if (permission != null)
                permissions.Add(new PermissionDto(
                    permission.Id,
                    permission.Resource,
                    permission.Action,
                    permission.Description,
                    permission.GetPermissionKey(),
                    permission.CreatedAt,
                    permission.UpdatedAt,
                    permission.DeletedAt
                ));
        }

        return new RoleWithPermissionsDto
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name,
            Description = role.Description,
            Rank = role.Rank,
            IsSystemRole = role.IsSystemRole,
            Permissions = permissions,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }
}
