using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Models;

/// <summary>
/// Model/Kiểu máy - Đầy đủ thông tin kỹ thuật và quản lý
/// </summary>
public class Model : Entity
{
    // Basic Information
    public string Name { get; private set; } = string.Empty;
    public string? ModelNumber { get; private set; } // Official model number (e.g., "XPS-15-9500")
    public string? SKU { get; private set; } // Stock Keeping Unit
    public string? PartNumber { get; private set; } // Part/Article number
    public string? Description { get; private set; }

    // Relationships
    public int? ManufacturerId { get; private set; }
    public int? CategoryId { get; private set; }
    public int? TypeId { get; private set; } // Asset Type

    // Product Information
    public string? ProductFamily { get; private set; } // e.g., "ThinkPad", "MacBook", "Surface"
    public string? Generation { get; private set; } // Gen 11, 2023, etc.
    public string? Series { get; private set; } // Professional, Business, Consumer
    public DateTime? ReleaseDate { get; private set; }
    public DateTime? DiscontinuedDate { get; private set; }
    public string? LifecycleStatus { get; private set; } // Current, Legacy, Discontinued, EOL

    // Technical Specifications (JSONB stored as string)
    public string? TechnicalSpecs { get; private set; } // Full JSON specs
    public string? Processor { get; private set; } // CPU info
    public string? Memory { get; private set; } // RAM info
    public string? Storage { get; private set; } // Storage info
    public string? Display { get; private set; } // Display specs
    public string? Graphics { get; private set; } // GPU info
    public string? OperatingSystem { get; private set; } // Default OS

    // Physical Specifications
    public decimal? Weight { get; private set; } // in kg
    public string? WeightUnit { get; private set; } // kg, lbs
    public string? Dimensions { get; private set; } // L x W x H
    public string? DimensionUnit { get; private set; } // cm, inches
    public string? Color { get; private set; }
    public string? Material { get; private set; }

    // Power & Environmental
    public string? PowerRequirements { get; private set; } // Voltage, wattage
    public decimal? PowerConsumption { get; private set; } // Watts
    public string? EnergyRating { get; private set; } // Energy Star, etc.
    public string? OperatingTemperature { get; private set; } // Temperature range
    public string? Humidity { get; private set; } // Humidity range

    // Connectivity & Ports
    public string? Connectivity { get; private set; } // JSON array: WiFi, Bluetooth, Ethernet, etc.
    public string? Ports { get; private set; } // JSON array: USB-C, HDMI, etc.
    public string? NetworkInterfaces { get; private set; }

    // Warranty & Support
    public int? StandardWarrantyMonths { get; private set; }
    public string? WarrantyType { get; private set; } // Onsite, Mail-in, Depot
    public Url? SupportDocumentUrl { get; private set; }
    public Url? UserManualUrl { get; private set; }
    public Url? QuickStartGuideUrl { get; private set; }

    // Pricing & Availability
    public decimal? MSRP { get; private set; } // Manufacturer's Suggested Retail Price
    public string? MSRPCurrency { get; private set; }
    public decimal? AverageCost { get; private set; }
    public string? CostCurrency { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public string? AvailabilityStatus { get; private set; } // In Stock, Back Order, Discontinued

    // Compliance & Certifications
    public string? Certifications { get; private set; } // JSON array: CE, FCC, RoHS, etc.
    public string? ComplianceStandards { get; private set; } // JSON array
    public bool IsRoHSCompliant { get; private set; }
    public bool IsEnergyStarCertified { get; private set; }
    public bool IsEPEATCertified { get; private set; }

    // Depreciation & Lifecycle
    public int? UsefulLifeMonths { get; private set; } // Expected useful life
    public string? DepreciationMethod { get; private set; } // Straight Line, Declining Balance
    public decimal? ResidualValuePercentage { get; private set; } // % of original cost

    // Software & Licensing (for software models)
    public string? LicenseType { get; private set; } // Perpetual, Subscription, Volume
    public int? LicenseDurationMonths { get; private set; }
    public bool RequiresActivation { get; private set; }
    public int? MaxInstallations { get; private set; }

    // Accessories & Bundles
    public string? IncludedAccessories { get; private set; } // JSON array
    public string? OptionalAccessories { get; private set; } // JSON array
    public string? RequiredAccessories { get; private set; } // JSON array
    public string? CompatibleModels { get; private set; } // JSON array of compatible model IDs

    // Media & Resources
    public Url? ImageUrl { get; private set; }
    public Url? ThumbnailUrl { get; private set; }
    public Url? ProductPageUrl { get; private set; }
    public Url? DatasheetUrl { get; private set; }
    public Url? VideoUrl { get; private set; }

    // Status & Flags
    public bool IsActive { get; private set; } = true;
    public bool IsDepreciable { get; private set; } = true;
    public bool IsTangible { get; private set; } = true;
    public bool RequiresLicense { get; private set; }
    public bool RequiresMaintenance { get; private set; } = true;

    // Internal Management
    public string? InternalNotes { get; private set; }
    public string? ProcurementNotes { get; private set; }
    public int? ReorderLevel { get; private set; }
    public int? CurrentStock { get; private set; }
    public DateTime? LastOrderDate { get; private set; }

    // SEO & Search
    public string? Tags { get; private set; } // JSON array for search/filtering
    public string? Keywords { get; private set; }

    // Navigation properties
    public Manufacturers.Manufacturer? Manufacturer { get; set; }
    public Categories.AssetCategory? Category { get; set; }
    public Types.AssetType? Type { get; set; }
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();

    private Model()
    {
    }

    public static Model Create(
        string name,
        int? manufacturerId = null,
        int? categoryId = null,
        string? modelNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Model name is required");

        return new Model
        {
            Name = name,
            ManufacturerId = manufacturerId,
            CategoryId = categoryId,
            ModelNumber = modelNumber,
            IsActive = true,
            IsDepreciable = true,
            IsTangible = true,
            RequiresMaintenance = true
        };
    }

    public void UpdateBasicInfo(
        string name,
        string? modelNumber,
        string? sku,
        string? partNumber,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Model name is required");

        Name = name;
        ModelNumber = modelNumber;
        SKU = sku;
        PartNumber = partNumber;
        Description = description;
    }

    public void UpdateProductInfo(
        string? productFamily,
        string? generation,
        string? series,
        DateTime? releaseDate,
        string? lifecycleStatus)
    {
        ProductFamily = productFamily;
        Generation = generation;
        Series = series;
        ReleaseDate = releaseDate;
        LifecycleStatus = lifecycleStatus;
    }

    public void UpdateTechnicalSpecs(
        string? technicalSpecs,
        string? processor,
        string? memory,
        string? storage,
        string? display,
        string? graphics,
        string? os)
    {
        TechnicalSpecs = technicalSpecs;
        Processor = processor;
        Memory = memory;
        Storage = storage;
        Display = display;
        Graphics = graphics;
        OperatingSystem = os;
    }

    public void UpdatePhysicalSpecs(
        decimal? weight,
        string? weightUnit,
        string? dimensions,
        string? dimensionUnit,
        string? color,
        string? material)
    {
        if (weight.HasValue && weight.Value < 0)
            throw new DomainException("Weight cannot be negative");

        Weight = weight;
        WeightUnit = weightUnit;
        Dimensions = dimensions;
        DimensionUnit = dimensionUnit;
        Color = color;
        Material = material;
    }

    public void UpdatePowerEnvironmental(
        string? powerRequirements,
        decimal? powerConsumption,
        string? energyRating,
        string? operatingTemperature,
        string? humidity)
    {
        if (powerConsumption.HasValue && powerConsumption.Value < 0)
            throw new DomainException("Power consumption cannot be negative");

        PowerRequirements = powerRequirements;
        PowerConsumption = powerConsumption;
        EnergyRating = energyRating;
        OperatingTemperature = operatingTemperature;
        Humidity = humidity;
    }

    public void UpdateConnectivity(
        string? connectivity,
        string? ports,
        string? networkInterfaces)
    {
        Connectivity = connectivity;
        Ports = ports;
        NetworkInterfaces = networkInterfaces;
    }

    public void UpdateWarrantySupport(
        int? standardWarrantyMonths,
        string? warrantyType,
        string? supportDocUrl,
        string? manualUrl,
        string? quickStartUrl)
    {
        if (standardWarrantyMonths.HasValue && standardWarrantyMonths.Value < 0)
            throw new DomainException("Standard warranty months cannot be negative");

        StandardWarrantyMonths = standardWarrantyMonths;
        WarrantyType = warrantyType;
        SupportDocumentUrl = supportDocUrl != null ? Url.Create(supportDocUrl) : null;
        UserManualUrl = manualUrl != null ? Url.Create(manualUrl) : null;
        QuickStartGuideUrl = quickStartUrl != null ? Url.Create(quickStartUrl) : null;
    }

    public void UpdatePricing(
        decimal? msrp,
        string? msrpCurrency,
        decimal? averageCost,
        string? costCurrency)
    {
        if (msrp.HasValue && msrp.Value < 0)
            throw new DomainException("MSRP cannot be negative");

        if (averageCost.HasValue && averageCost.Value < 0)
            throw new DomainException("Average cost cannot be negative");

        MSRP = msrp;
        MSRPCurrency = msrpCurrency;
        AverageCost = averageCost;
        CostCurrency = costCurrency;
    }

    public void UpdateAvailability(
        bool isAvailable,
        string? availabilityStatus,
        DateTime? discontinuedDate)
    {
        IsAvailable = isAvailable;
        AvailabilityStatus = availabilityStatus;
        DiscontinuedDate = discontinuedDate;
    }

    public void UpdateCompliance(
        string? certifications,
        string? complianceStandards,
        bool isRoHS,
        bool isEnergyStar,
        bool isEPEAT)
    {
        Certifications = certifications;
        ComplianceStandards = complianceStandards;
        IsRoHSCompliant = isRoHS;
        IsEnergyStarCertified = isEnergyStar;
        IsEPEATCertified = isEPEAT;
    }

    public void UpdateDepreciation(
        int? usefulLifeMonths,
        string? depreciationMethod,
        decimal? residualValuePercentage)
    {
        if (usefulLifeMonths.HasValue && usefulLifeMonths.Value < 0)
            throw new DomainException("Useful life months cannot be negative");

        if (residualValuePercentage.HasValue &&
            (residualValuePercentage.Value < 0 || residualValuePercentage.Value > 100))
            throw new DomainException("Residual value percentage must be between 0 and 100");

        UsefulLifeMonths = usefulLifeMonths;
        DepreciationMethod = depreciationMethod;
        ResidualValuePercentage = residualValuePercentage;
    }

    public void UpdateSoftwareLicensing(
        string? licenseType,
        int? licenseDuration,
        bool requiresActivation,
        int? maxInstallations)
    {
        if (licenseDuration.HasValue && licenseDuration.Value < 0)
            throw new DomainException("License duration months cannot be negative");

        if (maxInstallations.HasValue && maxInstallations.Value < 0)
            throw new DomainException("Maximum installations cannot be negative");

        LicenseType = licenseType;
        LicenseDurationMonths = licenseDuration;
        RequiresActivation = requiresActivation;
        MaxInstallations = maxInstallations;
    }

    public void UpdateAccessories(
        string? includedAccessories,
        string? optionalAccessories,
        string? requiredAccessories,
        string? compatibleModels)
    {
        IncludedAccessories = includedAccessories;
        OptionalAccessories = optionalAccessories;
        RequiredAccessories = requiredAccessories;
        CompatibleModels = compatibleModels;
    }

    public void UpdateMedia(
        string? imageUrl,
        string? thumbnailUrl,
        string? productPageUrl,
        string? datasheetUrl,
        string? videoUrl)
    {
        ImageUrl = imageUrl != null ? Url.Create(imageUrl) : null;
        ThumbnailUrl = thumbnailUrl != null ? Url.Create(thumbnailUrl) : null;
        ProductPageUrl = productPageUrl != null ? Url.Create(productPageUrl) : null;
        DatasheetUrl = datasheetUrl != null ? Url.Create(datasheetUrl) : null;
        VideoUrl = videoUrl != null ? Url.Create(videoUrl) : null;
    }

    public void UpdateFlags(
        bool isDepreciable,
        bool isTangible,
        bool requiresLicense,
        bool requiresMaintenance)
    {
        IsDepreciable = isDepreciable;
        IsTangible = isTangible;
        RequiresLicense = requiresLicense;
        RequiresMaintenance = requiresMaintenance;
    }

    public void UpdateInventory(
        int? reorderLevel,
        int? currentStock,
        DateTime? lastOrderDate)
    {
        if (reorderLevel.HasValue && reorderLevel.Value < 0)
            throw new DomainException("Reorder level cannot be negative");

        if (currentStock.HasValue && currentStock.Value < 0)
            throw new DomainException("Current stock cannot be negative");

        ReorderLevel = reorderLevel;
        CurrentStock = currentStock;
        LastOrderDate = lastOrderDate;
    }

    public void UpdateSearchTags(
        string? tags,
        string? keywords)
    {
        Tags = tags;
        Keywords = keywords;
    }

    public void AddNotes(string? internalNotes, string? procurementNotes)
    {
        InternalNotes = internalNotes;
        ProcurementNotes = procurementNotes;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Discontinue(DateTime? discontinuedDate = null)
    {
        IsAvailable = false;
        LifecycleStatus = "Discontinued";
        DiscontinuedDate = discontinuedDate ?? DateTime.UtcNow;
    }

    public bool IsEndOfLife()
    {
        return LifecycleStatus == "EOL" || LifecycleStatus == "Discontinued";
    }

    public bool NeedsReorder()
    {
        return ReorderLevel.HasValue && CurrentStock.HasValue && CurrentStock.Value <= ReorderLevel.Value;
    }
}