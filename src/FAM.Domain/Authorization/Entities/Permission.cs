using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Authorization permission
/// </summary>
public class Permission : BaseEntity
{
    public ResourceType Resource { get; private set; } = null!;
    public ResourceAction Action { get; private set; } = null!;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    private Permission() { }

    public static Permission Create(string resource, string action)
    {
        return new Permission
        {
            Resource = ResourceType.Create(resource),
            Action = ResourceAction.Create(action)
        };
    }
}