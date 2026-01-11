using FAM.Domain.Authorization.Events;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Authorization role - Aggregate Root
/// Manages role information and permissions
/// Uses FullAuditedAggregateRoot for complete audit trail
/// </summary>
public class Role : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier, IHasDeletionTime,
    IHasDeleter, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Rank { get; private set; }
    public bool IsSystemRole { get; private set; }
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
        {
            throw new DomainException(ErrorCodes.ROLE_NAME_REQUIRED, "Role name cannot be empty");
        }

        if (rank < 0)
        {
            throw new DomainException(ErrorCodes.ROLE_INVALID_RANK, "Role rank must be non-negative");
        }

        // Validate code
        RoleCode roleCodeVo = RoleCode.Create(code);

        Role role = new()
        {
            Code = roleCodeVo.Value,
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
        {
            throw new DomainException(ErrorCodes.ROLE_NAME_REQUIRED, "Role name cannot be empty");
        }

        if (rank < 0)
        {
            throw new DomainException(ErrorCodes.ROLE_INVALID_RANK, "Role rank must be non-negative");
        }

        if (IsSystemRole)
        {
            throw new DomainException(ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_UPDATE, "System roles cannot be updated");
        }

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
        {
            throw new DomainException(ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_DELETE, "System roles cannot be deleted");
        }
    }

    /// <summary>
    /// Assign permissions to this role
    /// </summary>
    public void AssignPermissions(IEnumerable<Permission> permissions)
    {
        List<Permission> permissionList = permissions.ToList();
        if (!permissionList.Any())
        {
            throw new DomainException(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED, "No permissions provided");
        }

        List<long> permissionIds = permissionList.Select(p => p.Id).ToList();
        RaiseDomainEvent(new PermissionsAssignedToRole(Id, permissionIds));
    }

    /// <summary>
    /// Revoke permissions from this role
    /// </summary>
    public void RevokePermissions(IEnumerable<long> permissionIds)
    {
        List<long> idList = permissionIds.ToList();
        if (!idList.Any())
        {
            throw new DomainException(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED, "No permission IDs provided");
        }

        RaiseDomainEvent(new PermissionsRevokedFromRole(Id, idList));
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
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
