using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Location
/// </summary>
[Table("locations")]
public class LocationEf : BaseEntityEf
{
    public string Name { get; set; } = string.Empty;
    public long? CompanyId { get; set; }
    public string? Code { get; set; }
    public long? ParentId { get; set; }
    public string? Type { get; set; }
    public string? FullPath { get; set; }
    public string? PathIds { get; set; }
    public long? CountryId { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public CompanyDetailsEf? Company { get; set; }
    public CountryEf? Country { get; set; }
    public LocationEf? Parent { get; set; }
    public ICollection<LocationEf> Children { get; set; } = new List<LocationEf>();
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}