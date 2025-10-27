using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Organizations;

/// <summary>
/// Company-specific details
/// </summary>
public class CompanyDetails : Entity
{
    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public TaxCode? TaxCode { get; private set; }
    public DomainName? Domain { get; private set; }
    public Address? Address { get; private set; }
    public DateTime? EstablishedOn { get; private set; }

    private CompanyDetails() { }

    public static CompanyDetails Create(string? taxCode = null, string? domain = null, string? address = null, DateTime? establishedOn = null)
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
}