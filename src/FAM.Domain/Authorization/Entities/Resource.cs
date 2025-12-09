using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Organizations;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Business resource
/// Uses BasicAuditedEntity for basic audit trail
/// </summary>
public class Resource : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasDeletionTime
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public ResourceType Type { get; private set; } = null!;
    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;

    private Resource()
    {
    }

    public static Resource Create(string type, OrgNode node, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Resource name cannot be empty");

        return new Resource
        {
            Type = ResourceType.Create(type),
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