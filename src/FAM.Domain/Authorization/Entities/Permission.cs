using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Authorization permission
/// Defines what actions can be performed on which resources
/// Uses FullAuditedEntity for complete audit trail
/// </summary>
public class Permission : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
    public User? DeletedBy { get; set; }

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

    public virtual void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
