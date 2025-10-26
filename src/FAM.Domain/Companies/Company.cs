using FAM.Domain.Common;

namespace FAM.Domain.Companies;

/// <summary>
/// Công ty
/// </summary>
public class Company : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? TaxCode { get; private set; }
    public string? Address { get; private set; }

    // Navigation properties
    public ICollection<Locations.Location> Locations { get; set; } = new List<Locations.Location>();
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();
    public ICollection<Suppliers.Supplier> Suppliers { get; set; } = new List<Suppliers.Supplier>();

    private Company() { }

    public static Company Create(string name, string? taxCode = null, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Company name cannot be empty");

        return new Company
        {
            Name = name.Trim(),
            TaxCode = taxCode,
            Address = address
        };
    }

    public void Update(string name, string? taxCode, string? address)
    {
        Name = name;
        TaxCode = taxCode;
        Address = address;
    }
}
