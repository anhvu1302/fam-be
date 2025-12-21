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
    public string? TaxCode { get; private set; }
    public string? Domain { get; private set; }
    public Address? Address { get; private set; }
    public DateTime? EstablishedOn { get; private set; }

    private CompanyDetails()
    {
    }

    public static CompanyDetails Create(string? taxCode = null, string? domain = null, string? address = null,
        DateTime? establishedOn = null)
    {
        var companyDetails = new CompanyDetails
        {
            Address = address != null ? Address.Create(address, "Unknown", "VN") : null,
            EstablishedOn = establishedOn
        };

        companyDetails.TaxCode = companyDetails.ValidateTaxCode(taxCode);
        companyDetails.Domain = companyDetails.ValidateDomainName(domain);

        return companyDetails;
    }

    internal void SetNode(OrgNode node)
    {
        NodeId = node.Id;
        Node = node;
    }

    public void Update(string? taxCode, string? domain, string? address, DateTime? establishedOn)
    {
        TaxCode = ValidateTaxCode(taxCode);
        Domain = ValidateDomainName(domain);
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

    // Private helper methods
    private string? ValidateTaxCode(string? taxCode)
    {
        if (string.IsNullOrWhiteSpace(taxCode))
            return null;

        var taxCodeVo = ValueObjects.TaxCode.Create(taxCode);
        return taxCodeVo.Value;
    }

    private string? ValidateDomainName(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return null;

        var domainVo = DomainName.Create(domain);
        return domainVo.Value;
    }
}
