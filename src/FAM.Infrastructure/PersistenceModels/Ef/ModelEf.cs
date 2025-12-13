using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Model
/// </summary>
[Table("models")]
public class ModelEf : BaseEntityEf
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string? ModelNumber { get; set; }
    public string? SKU { get; set; }
    public string? PartNumber { get; set; }
    public string? Description { get; set; }

    // Relationships
    public long? ManufacturerId { get; set; }
    public long? CategoryId { get; set; }
    public long? TypeId { get; set; }

    // Product Information
    public string? ProductFamily { get; set; }
    public string? Generation { get; set; }
    public string? Series { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    public string? LifecycleStatus { get; set; }

    // Technical Specifications
    public string? TechnicalSpecs { get; set; }
    public string? Processor { get; set; }
    public string? Memory { get; set; }
    public string? Storage { get; set; }
    public string? Display { get; set; }
    public string? Graphics { get; set; }
    public string? OperatingSystem { get; set; }

    // Physical Specifications
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public string? Dimensions { get; set; }
    public string? DimensionUnit { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }

    // Power & Environmental
    public string? PowerRequirements { get; set; }
    public decimal? PowerConsumption { get; set; }
    public string? EnergyRating { get; set; }
    public string? OperatingTemperature { get; set; }
    public string? Humidity { get; set; }

    // Connectivity & Ports
    public string? Connectivity { get; set; }
    public string? Ports { get; set; }
    public string? NetworkInterfaces { get; set; }

    // Warranty & Support
    public int? StandardWarrantyMonths { get; set; }
    public string? WarrantyType { get; set; }
    public string? SupportDocumentUrl { get; set; }
    public string? UserManualUrl { get; set; }
    public string? QuickStartGuideUrl { get; set; }

    // Pricing & Availability
    public decimal? MSRP { get; set; }
    public string? Currency { get; set; }
    public string? MSRPCurrency { get; set; }
    public decimal? AverageCost { get; set; }
    public string? CostCurrency { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? AvailabilityStatus { get; set; }

    // Compliance & Certifications
    public string? Certifications { get; set; }
    public string? ComplianceStandards { get; set; }
    public bool IsRoHSCompliant { get; set; }
    public bool IsEnergyStarCertified { get; set; }
    public bool IsEPEATCertified { get; set; }

    // Depreciation & Lifecycle
    public int? UsefulLifeMonths { get; set; }
    public string? DepreciationMethod { get; set; }
    public decimal? ResidualValuePercentage { get; set; }

    // Software & Licensing
    public string? LicenseType { get; set; }
    public int? LicenseDurationMonths { get; set; }
    public bool RequiresActivation { get; set; }
    public int? MaxInstallations { get; set; }

    // Accessories & Bundles
    public string? IncludedAccessories { get; set; }
    public string? OptionalAccessories { get; set; }
    public string? RequiredAccessories { get; set; }
    public string? CompatibleModels { get; set; }

    // Media & Resources
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? ProductPageUrl { get; set; }
    public string? DatasheetUrl { get; set; }
    public string? VideoUrl { get; set; }

    // Status & Flags
    public bool IsActive { get; set; } = true;
    public bool IsDepreciable { get; set; } = true;
    public bool IsTangible { get; set; } = true;
    public bool RequiresLicense { get; set; }
    public bool RequiresMaintenance { get; set; } = true;

    // Internal Management
    public string? InternalNotes { get; set; }
    public string? ProcurementNotes { get; set; }
    public int? ReorderLevel { get; set; }
    public int? CurrentStock { get; set; }
    public DateTime? LastOrderDate { get; set; }

    // SEO & Search
    public string? Tags { get; set; }

    public string? Keywords { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }


    // Navigation properties
    public ManufacturerEf? Manufacturer { get; set; }
    public AssetCategoryEf? Category { get; set; }
    public AssetTypeEf? Type { get; set; }
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}
