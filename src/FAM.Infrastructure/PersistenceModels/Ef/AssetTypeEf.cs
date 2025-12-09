using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for AssetType
/// </summary>
[Table("asset_types")]
public class AssetTypeEf : BaseEntityEf
{
    // Basic Information
    [Required] [MaxLength(20)] public string Code { get; set; } = string.Empty;

    [Required] [MaxLength(200)] public string Name { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    [MaxLength(2000)] public string? LongDescription { get; set; }

    // Hierarchy
    public long? ParentId { get; set; }
    public int Level { get; set; }

    [MaxLength(1000)] public string? Path { get; set; }

    // Classification
    [MaxLength(100)] public string? AssetClass { get; set; }

    [MaxLength(100)] public string? Category { get; set; }

    [MaxLength(100)] public string? Subcategory { get; set; }

    // Properties & Characteristics
    public bool IsDepreciable { get; set; } = true;
    public bool IsAssignable { get; set; } = true;
    public bool IsTangible { get; set; } = true;
    public bool IsConsumable { get; set; }
    public bool IsCapitalized { get; set; } = true;
    public bool RequiresLicense { get; set; }
    public bool RequiresMaintenance { get; set; } = true;
    public bool RequiresCalibration { get; set; }
    public bool RequiresInsurance { get; set; }
    public bool IsITAsset { get; set; }

    // Depreciation Defaults
    [MaxLength(100)] public string? DefaultDepreciationMethod { get; set; }

    public int? DefaultUsefulLifeMonths { get; set; }
    public decimal? DefaultResidualValuePercentage { get; set; }

    [MaxLength(50)] public string? DepreciationAccountCode { get; set; }

    [MaxLength(50)] public string? AccumulatedDepreciationAccountCode { get; set; }

    // Accounting
    [MaxLength(50)] public string? GLAccountCode { get; set; }

    [MaxLength(50)] public string? AssetAccountCode { get; set; }

    [MaxLength(50)] public string? ExpenseAccountCode { get; set; }

    [MaxLength(50)] public string? CostCenter { get; set; }

    // Lifecycle
    public int? DefaultWarrantyMonths { get; set; }
    public int? DefaultMaintenanceIntervalDays { get; set; }

    [MaxLength(100)] public string? DefaultMaintenanceType { get; set; }

    // Valuation
    [Column(TypeName = "decimal(18,2)")] public decimal? MinimumCapitalizationValue { get; set; }

    [MaxLength(3)] public string? ValuationCurrency { get; set; }

    [MaxLength(100)] public string? ValuationMethod { get; set; }

    // Compliance & Regulations
    public bool RequiresCompliance { get; set; }
    public string? ComplianceStandards { get; set; }
    public string? RegulatoryRequirements { get; set; }
    public bool RequiresAudit { get; set; }
    public int? AuditIntervalMonths { get; set; }

    // Security
    [MaxLength(50)] public string? DefaultSecurityClassification { get; set; }

    public bool RequiresBackgroundCheck { get; set; }
    public bool RequiresAccessControl { get; set; }

    // Workflow & Approval
    public bool RequiresApprovalToAcquire { get; set; }
    public bool RequiresApprovalToDispose { get; set; }
    public string? ApprovalWorkflow { get; set; }

    // Custom Fields Definition
    public string? CustomFieldsSchema { get; set; }
    public string? RequiredFields { get; set; }

    // Display & UI
    [MaxLength(100)] public string? IconName { get; set; }

    [MaxLength(500)] public string? IconUrl { get; set; }

    [MaxLength(20)] public string? Color { get; set; }

    public int DisplayOrder { get; set; }

    // Status & Metadata
    public bool IsActive { get; set; } = true;
    public bool IsSystemType { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Tags & Search
    public string? Tags { get; set; }
    public string? SearchKeywords { get; set; }
    public string? Aliases { get; set; }

    // Statistics
    public int AssetCount { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal? TotalValue { get; set; }

    // Internal Notes
    public string? InternalNotes { get; set; }

    public string? ProcurementNotes { get; set; }

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
    [ForeignKey("ParentId")] public AssetTypeEf? Parent { get; set; }

    public ICollection<AssetTypeEf> Children { get; set; } = new List<AssetTypeEf>();
    public ICollection<ModelEf> Models { get; set; } = new List<ModelEf>();
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}