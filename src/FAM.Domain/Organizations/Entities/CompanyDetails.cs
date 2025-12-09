using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Organizations;

/// <summary>
/// Company-specific details
/// Uses BasicAuditedEntity for basic audit trail
/// </summary>
public class CompanyDetails : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasDeletionTime
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public TaxCode? TaxCode { get; private set; }
    public DomainName? Domain { get; private set; }
    public Address? Address { get; private set; }
    public DateTime? EstablishedOn { get; private set; }

    private CompanyDetails()
    {
    }

    public static CompanyDetails Create(string? taxCode = null, string? domain = null, string? address = null,
        DateTime? establishedOn = null)
    {
        return new CompanyDetails
        {
            TaxCode = taxCode != null ? TaxCode.Create(taxCode) : null,
            Domain = domain != null ? DomainName.Create(domain) : null,
            Address = address != null ? Address.Create(address, "Unknown", "VN") : null,
            EstablishedOn = establishedOn
        };
    }

    internal void SetNode(OrgNode node)
    {
        NodeId = node.Id;
        Node = node;
    }

    public void Update(string? taxCode, string? domain, string? address, DateTime? establishedOn)
    {
        TaxCode = taxCode != null ? TaxCode.Create(taxCode) : null;
        Domain = domain != null ? DomainName.Create(domain) : null;
        Address = address != null ? Address.Create(address, "Unknown", "VN") : null;
        EstablishedOn = establishedOn;
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