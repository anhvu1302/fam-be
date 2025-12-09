using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;

namespace FAM.Domain.Organizations;

/// <summary>
/// Organizational node in the hierarchy: Company -> Department -> Group
/// Uses FullAuditedAggregateRoot for complete audit trail
/// </summary>
public class OrgNode : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier, IHasDeletionTime,
    IHasDeleter, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

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

    public OrgNodeType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public long? ParentId { get; private set; }
    public OrgNode? Parent { get; private set; }
    public ICollection<OrgNode> Children { get; private set; } = new List<OrgNode>();

    // Navigation for different types
    public CompanyDetails? CompanyDetails { get; private set; }
    public DepartmentDetails? DepartmentDetails { get; private set; }

    // Authorization
    public ICollection<UserNodeRole> UserNodeRoles { get; set; } = new List<UserNodeRole>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();

    private OrgNode()
    {
    }

    public static OrgNode CreateCompany(string name, CompanyDetails details)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("OrgNode name cannot be empty");

        var node = new OrgNode
        {
            Type = OrgNodeType.Company,
            Name = name.Trim(),
            ParentId = null
        };

        details.SetNode(node);
        node.CompanyDetails = details;

        return node;
    }

    public static OrgNode CreateDepartment(string name, DepartmentDetails details, OrgNode parent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("OrgNode name cannot be empty");

        if (parent.Type != OrgNodeType.Company)
            throw new DomainException("Department parent must be a Company");

        var node = new OrgNode
        {
            Type = OrgNodeType.Department,
            Name = name.Trim(),
            ParentId = parent.Id,
            Parent = parent
        };

        details.SetNode(node);
        node.DepartmentDetails = details;

        return node;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("OrgNode name cannot be empty");

        Name = name.Trim();
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