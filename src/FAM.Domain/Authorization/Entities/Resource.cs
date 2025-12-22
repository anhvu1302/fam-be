using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Organizations;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Business resource
/// Uses BasicAuditedEntity for basic audit trail
/// </summary>
public class Resource : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasDeletionTime
{
    public string Type { get; private set; } = null!;
    public long NodeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public OrgNode Node { get; private set; } = null!;

    private Resource()
    {
    }

    public static Resource Create(string type, OrgNode node, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Resource name cannot be empty");

        // Validate type
        var resourceTypeVo = ResourceType.Create(type);

        return new Resource
        {
            Type = resourceTypeVo.Value,
            NodeId = node.Id,
            Node = node,
            Name = name.Trim()
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Resource name cannot be empty");

        Name = name.Trim();
    }

    public virtual void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
