using FAM.Domain.Assets;
using FAM.Domain.Common;
using FAM.Domain.Locations;
using FAM.Domain.Manufacturers;
using FAM.Domain.Suppliers;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Geography;

/// <summary>
/// Quốc gia - Theo chuẩn ISO 3166
/// </summary>
public class Country : Entity
{
    // ISO Codes
    public CountryCode Code { get; private set; } = null!; // ISO 3166-1 alpha-2 (VN, US, JP)
    public string? Iso3Code { get; private set; } // ISO 3166-1 alpha-3 (VNM, USA, JPN)
    public string? NumericCode { get; private set; } // ISO 3166-1 numeric (704, 840, 392)

    // Names
    public string Name { get; private set; } = string.Empty; // Official name in English
    public string? NativeName { get; private set; } // Native name (Việt Nam, United States)
    public string? OfficialName { get; private set; } // Full official name

    // Regional Information
    public string? Region { get; private set; } // Asia, Europe, Americas, Africa, Oceania
    public string? SubRegion { get; private set; } // South-Eastern Asia, Northern Europe, etc.
    public string? Continent { get; private set; } // AS, EU, NA, SA, AF, OC, AN

    // Location
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    // Communication
    public string? PhoneCode { get; private set; } // +84, +1, +81
    public string? TLD { get; private set; } // Top Level Domain (.vn, .us, .jp)

    // Currency
    public string? CurrencyCode { get; private set; } // ISO 4217 (VND, USD, JPY)
    public string? CurrencyName { get; private set; }
    public string? CurrencySymbol { get; private set; } // ₫, $, ¥

    // Language
    public string? PrimaryLanguage { get; private set; } // ISO 639-1 (vi, en, ja)
    public string? Languages { get; private set; } // JSON array of supported languages

    // Administrative
    public string? Capital { get; private set; } // Capital city
    public string? Nationality { get; private set; } // Vietnamese, American, Japanese
    public string? TimeZones { get; private set; } // JSON array (UTC+07:00, UTC-05:00, etc.)

    // Status & Properties
    public bool IsActive { get; private set; } = true;
    public bool IsEUMember { get; private set; } // European Union member
    public bool IsUNMember { get; private set; } = true; // United Nations member
    public bool IsIndependent { get; private set; } = true;

    // Additional Info
    public string? Flag { get; private set; } // Flag emoji or URL
    public string? CoatOfArms { get; private set; } // URL to coat of arms image
    public long? Population { get; private set; }
    public decimal? Area { get; private set; } // Area in km²

    // Navigation properties
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();

    private Country()
    {
    }

    public static Country Create(
        string code,
        string name,
        string? iso3Code = null,
        string? numericCode = null)
    {
        return new Country
        {
            Code = CountryCode.Create(code),
            Name = name,
            Iso3Code = iso3Code?.ToUpperInvariant(),
            NumericCode = numericCode
        };
    }

    public void UpdateBasicInfo(
        string name,
        string? nativeName,
        string? officialName)
    {
        Name = name;
        NativeName = nativeName;
        OfficialName = officialName;
    }

    public void UpdateRegionalInfo(
        string? region,
        string? subRegion,
        string? continent,
        decimal? latitude,
        decimal? longitude)
    {
        Region = region;
        SubRegion = subRegion;
        Continent = continent;
        Latitude = latitude;
        Longitude = longitude;
    }

    public void UpdateCommunication(
        string? phoneCode,
        string? tld)
    {
        PhoneCode = phoneCode;
        TLD = tld;
    }

    public void UpdateCurrency(
        string? currencyCode,
        string? currencyName,
        string? currencySymbol)
    {
        CurrencyCode = currencyCode;
        CurrencyName = currencyName;
        CurrencySymbol = currencySymbol;
    }

    public void UpdateLanguage(
        string? primaryLanguage,
        string? languages)
    {
        PrimaryLanguage = primaryLanguage;
        Languages = languages;
    }

    public void UpdateAdministrative(
        string? capital,
        string? nationality,
        string? timeZones)
    {
        Capital = capital;
        Nationality = nationality;
        TimeZones = timeZones;
    }

    public void UpdateStatus(
        bool isActive,
        bool isEUMember,
        bool isUNMember,
        bool isIndependent)
    {
        IsActive = isActive;
        IsEUMember = isEUMember;
        IsUNMember = isUNMember;
        IsIndependent = isIndependent;
    }

    public void UpdateAdditionalInfo(
        string? flag,
        string? coatOfArms,
        long? population,
        decimal? area)
    {
        Flag = flag;
        CoatOfArms = coatOfArms;
        Population = population;
        Area = area;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}