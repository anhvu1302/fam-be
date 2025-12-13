using FAM.Domain.Assets;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Geography;
using FAM.Domain.Organizations;
using FAM.Domain.Users;

namespace FAM.Domain.Locations;

/// <summary>
/// Địa điểm/vị trí (hỗ trợ phân cấp hierarchical tree) - Aggregate Root
/// Uses FullAuditedAggregateRoot for complete audit trail
/// </summary>
public class Location : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier, IHasDeletionTime,
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

    public string Name { get; private set; } = string.Empty;
    public int? CompanyId { get; private set; }
    public string? Code { get; private set; }
    public int? ParentId { get; private set; }
    public string? Type { get; private set; }
    public string? FullPath { get; private set; }
    public string? PathIds { get; private set; }
    public int? CountryId { get; private set; }
    public string? Description { get; private set; }

    // Navigation properties
    public CompanyDetails? Company { get; set; }
    public Country? Country { get; set; }
    public Location? Parent { get; set; }
    public ICollection<Location> Children { get; set; } = new List<Location>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();

    private Location()
    {
    }

    public static Location Create(string name, string? code = null, int? companyId = null, int? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Location name cannot be empty");

        return new Location
        {
            Name = name,
            Code = code,
            CompanyId = companyId,
            ParentId = parentId
        };
    }

    public void Update(string name, string? description, int? countryId = null)
    {
        Name = name;
        Description = description;
        CountryId = countryId;
    }

    public void SetParent(int? parentId)
    {
        ParentId = parentId;
    }

    public void BuildPath(string fullPath, string pathIds)
    {
        FullPath = fullPath;
        PathIds = pathIds;
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
