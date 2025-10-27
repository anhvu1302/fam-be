using FAM.Domain.Common;

namespace FAM.Domain.Authorization;

/// <summary>
/// Role-permission mapping
/// </summary>
public class RolePermission : Entity
{
    public long RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public long PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { }

    public static RolePermission Create(Role role, Permission permission)
    {
        return new RolePermission
        {
            RoleId = role.Id,
            Role = role,
            PermissionId = permission.Id,
            Permission = permission
        };
    }
}