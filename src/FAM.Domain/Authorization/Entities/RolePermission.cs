using FAM.Domain.Common;

namespace FAM.Domain.Authorization;

/// <summary>
/// Role-permission mapping
/// Represents which permissions are assigned to which roles
/// Composite key: (RoleId, PermissionId)
/// </summary>
public class RolePermission : JunctionEntity
{
    public long RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public long PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;
    public long? GrantedById { get; private set; }

    private RolePermission()
    {
    }

    /// <summary>
    /// Create role-permission assignment
    /// </summary>
    public static RolePermission Create(long roleId, long permissionId, long? grantedById = null)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedById = grantedById
        };
    }

    /// <summary>
    /// Create role-permission assignment with entities
    /// </summary>
    public static RolePermission Create(Role role, Permission permission, long? grantedById = null)
    {
        return new RolePermission
        {
            RoleId = role.Id,
            Role = role,
            PermissionId = permission.Id,
            Permission = permission,
            GrantedById = grantedById
        };
    }
}