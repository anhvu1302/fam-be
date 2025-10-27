using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Asset
/// </summary>
[Table("assets")]
public class AssetEf : BaseEntityEf
{
    public string Name { get; set; } = string.Empty;

    // Company & Type
    public long? CompanyId { get; set; }
    public long? AssetTypeId { get; set; }
    public long? CategoryId { get; set; }

    // Model & Manufacturer
    public long? ModelId { get; set; }
    public long? ManufacturerId { get; set; }

    // Identification
    public string? SerialNo { get; set; }
    public string? AssetTag { get; set; }
    public string? Barcode { get; set; }
    public string? QRCode { get; set; }
    public string? RFIDTag { get; set; }

    // Purchase Information
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? PurchaseOrderNo { get; set; }
    public string? InvoiceNo { get; set; }
    public long? SupplierId { get; set; }
    public DateTime? WarrantyUntil { get; set; }
    public int? WarrantyMonths { get; set; }
    public string? WarrantyTerms { get; set; }

    // Condition & Location
    public long? ConditionId { get; set; }
    public long? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public long? CountryId { get; set; }

    // Assignment
    public int? DepartmentId { get; set; }
    public long? OwnerId { get; set; }

    // Status
    public long? UsageStatusId { get; set; }
    public long? LifecycleStatusId { get; set; }

    // Financial
    public decimal? CurrentValue { get; set; }
    public decimal? NetBookValue { get; set; }

    // Maintenance
    public int? MaintenanceIntervalDays { get; set; }

    // Custom Fields
    public string? CustomFields { get; set; }

    // Audit
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }

    // Depreciation & Finance
    public string? DepreciationMethod { get; set; }
    public int? UsefulLifeMonths { get; set; }
    public decimal? ResidualValue { get; set; }
    public DateTime? InServiceDate { get; set; }
    public decimal? CurrentBookValue { get; set; }
    public decimal? AccumulatedDepreciation { get; set; }
    public DateTime? LastDepreciationDate { get; set; }
    public string? AccountingCode { get; set; }
    public string? CostCenter { get; set; }
    public string? GLAccount { get; set; }

    // Insurance & Risk
    public string? InsurancePolicyNo { get; set; }
    public decimal? InsuredValue { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public string? RiskLevel { get; set; }

    // Maintenance & Support
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceContractNo { get; set; }
    public DateTime? SupportExpiryDate { get; set; }
    public string? ServiceLevel { get; set; }

    // IT/Software Specific
    public string? IPAddress { get; set; }
    public string? MACAddress { get; set; }
    public string? Hostname { get; set; }
    public string? OperatingSystem { get; set; }
    public string? SoftwareVersion { get; set; }
    public string? LicenseKey { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public int? LicenseCount { get; set; }

    // Physical Characteristics
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }

    // Energy & Environmental
    public decimal? PowerConsumption { get; set; }
    public string? EnergyRating { get; set; }
    public bool IsEnvironmentallyFriendly { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public string? DisposalMethod { get; set; }

    // Compliance & Security
    public string? ComplianceStatus { get; set; }
    public string? SecurityClassification { get; set; }
    public bool RequiresBackgroundCheck { get; set; }
    public string? DataClassification { get; set; }
    public DateTime? LastAuditDate { get; set; }
    public DateTime? NextAuditDate { get; set; }

    // Additional tracking
    public string? ProjectCode { get; set; }
    public string? CampaignCode { get; set; }
    public string? FundingSource { get; set; }
    public decimal? ReplacementCost { get; set; }
    public int? EstimatedRemainingLifeMonths { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Navigation properties
    public CompanyDetailsEf? Company { get; set; }
    public AssetTypeEf? AssetType { get; set; }
    public AssetCategoryEf? Category { get; set; }
    public ModelEf? Model { get; set; }
    public ManufacturerEf? Manufacturer { get; set; }
    public SupplierEf? Supplier { get; set; }
    public AssetConditionEf? Condition { get; set; }
    public LocationEf? Location { get; set; }
    public CountryEf? Country { get; set; }
    public UserEf? Owner { get; set; }
    public LifecycleStatusEf? LifecycleStatus { get; set; }
    public UsageStatusEf? UsageStatus { get; set; }

    public UserEf? CreatedByUser { get; set; }
    public UserEf? UpdatedByUser { get; set; }

    public ICollection<AssignmentEf> Assignments { get; set; } = new List<AssignmentEf>();
    public ICollection<AssetEventEf> AssetEvents { get; set; } = new List<AssetEventEf>();
    public ICollection<FinanceEntryEf> FinanceEntries { get; set; } = new List<FinanceEntryEf>();
    public ICollection<AttachmentEf> Attachments { get; set; } = new List<AttachmentEf>();
}