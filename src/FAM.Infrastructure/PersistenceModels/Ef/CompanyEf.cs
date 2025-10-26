using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Company
/// Separate from domain entity to avoid persistence concerns leaking into domain
/// </summary>
[Table("companies")]
public class CompanyEf : EntityEf
{
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }

    // Navigation properties for EF relationships
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
    public ICollection<LocationEf> Locations { get; set; } = new List<LocationEf>();
    public ICollection<SupplierEf> Suppliers { get; set; } = new List<SupplierEf>();
}