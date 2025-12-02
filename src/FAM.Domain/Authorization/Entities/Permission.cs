using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Authorization permission
/// Defines what actions can be performed on which resources
/// </summary>
public class Permission : BaseEntity
{
    public ResourceType Resource { get; private set; } = null!;
    public ResourceAction Action { get; private set; } = null!;
    public string? Description { get; private set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    private Permission()
    {
    }

    /// <summary>
    /// Create a new permission
    /// </summary>
    public static Permission Create(string resource, string action, string? description = null)
    {
        // Validate against defined permissions
        if (!Permissions.IsValidPermission(resource, action))
            throw new DomainException(
                ErrorCodes.PERMISSION_INVALID,
                $"Invalid permission: {resource}:{action}");

        return new Permission
        {
            Resource = ResourceType.Create(resource),
            Action = ResourceAction.Create(action),
            Description = description
        };
    }

    /// <summary>
    /// Get permission key in format: resource:action
    /// </summary>
    public string GetPermissionKey()
    {
        return $"{Resource.Value}:{Action.Value}";
    }
}