using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Country
/// </summary>
[Table("countries")]
public class CountryEf : EntityEf
{
    // ISO Codes
    public string Code { get; set; } = string.Empty;
    public string? Alpha3Code { get; set; }
    public string? NumericCode { get; set; }

    // Names
    public string Name { get; set; } = string.Empty;
    public string? NativeName { get; set; }
    public string? OfficialName { get; set; }

    // Regional Information
    public string? Region { get; set; }
    public string? SubRegion { get; set; }
    public string? Continent { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Communication
    public string? CallingCode { get; set; }
    public string? TopLevelDomain { get; set; }

    // Currency
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }

    // Language
    public string? PrimaryLanguage { get; set; }
    public string? Languages { get; set; }

    // Administrative
    public string? Capital { get; set; }
    public string? Nationality { get; set; }
    public string? TimeZones { get; set; }

    // Status & Properties
    public bool IsActive { get; set; } = true;
    public bool IsEUMember { get; set; }
    public bool IsUNMember { get; set; } = true;
    public bool IsIndependent { get; set; } = true;

    // Additional Info
    public string? Flag { get; set; }
    public string? CoatOfArms { get; set; }
    public long? Population { get; set; }
    public decimal? Area { get; set; }

    // Navigation properties
    public ICollection<LocationEf> Locations { get; set; } = new List<LocationEf>();
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
    public ICollection<SupplierEf> Suppliers { get; set; } = new List<SupplierEf>();
    public ICollection<ManufacturerEf> Manufacturers { get; set; } = new List<ManufacturerEf>();
}