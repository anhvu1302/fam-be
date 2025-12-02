using FAM.Domain.Authorization.Events;
using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Authorization role - Aggregate Root
/// Manages role information and permissions
/// </summary>
public class Role : AggregateRoot
{
    public RoleCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Rank { get; private set; }
    public bool IsSystemRole { get; private set; }

    // Navigation properties
    public ICollection<UserNodeRole> UserNodeRoles { get; set; } = new List<UserNodeRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    private Role()
    {
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    public static Role Create(string code, string name, int rank, string? description = null, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ErrorCodes.ROLE_NAME_REQUIRED, "Role name cannot be empty");

        if (rank < 0)
            throw new DomainException(ErrorCodes.ROLE_INVALID_RANK, "Role rank must be non-negative");

        var role = new Role
        {
            Code = RoleCode.Create(code),
            Name = name.Trim(),
            Description = description?.Trim(),
            Rank = rank,
            IsSystemRole = isSystemRole
        };

        role.RaiseDomainEvent(new RoleCreated(role.Id, code, role.Name, rank));
        return role;
    }

    /// <summary>
    /// Update role information
    /// </summary>
    public void Update(string name, int rank, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ErrorCodes.ROLE_NAME_REQUIRED, "Role name cannot be empty");

        if (rank < 0)
            throw new DomainException(ErrorCodes.ROLE_INVALID_RANK, "Role rank must be non-negative");

        if (IsSystemRole)
            throw new DomainException(ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_UPDATE, "System roles cannot be updated");

        Name = name.Trim();
        Description = description?.Trim();
        Rank = rank;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RoleUpdated(Id, Name, Rank));
    }

    /// <summary>
    /// Check if role can be deleted
    /// </summary>
    public void ValidateCanDelete()
    {
        if (IsSystemRole)
            throw new DomainException(ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_DELETE, "System roles cannot be deleted");
    }

    /// <summary>
    /// Assign permissions to this role
    /// </summary>
    public void AssignPermissions(IEnumerable<Permission> permissions)
    {
        var permissionList = permissions.ToList();
        if (!permissionList.Any())
            throw new DomainException(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED, "No permissions provided");

        var permissionIds = permissionList.Select(p => p.Id).ToList();
        RaiseDomainEvent(new PermissionsAssignedToRole(Id, permissionIds));
    }

    /// <summary>
    /// Revoke permissions from this role
    /// </summary>
    public void RevokePermissions(IEnumerable<long> permissionIds)
    {
        var idList = permissionIds.ToList();
        if (!idList.Any())
            throw new DomainException(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED, "No permission IDs provided");

        RaiseDomainEvent(new PermissionsRevokedFromRole(Id, idList));
    }
}