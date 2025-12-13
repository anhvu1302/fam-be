using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for AssetCategory
/// </summary>
[Table("asset_categories")]
public class AssetCategoryEf : BaseEntityEf
{
    // Basic Information
    [Required] [MaxLength(200)] public string Name { get; set; } = string.Empty;

    [MaxLength(50)] public string? Code { get; set; }

    [MaxLength(500)] public string? Description { get; set; }

    [MaxLength(2000)] public string? LongDescription { get; set; }

    // Hierarchy
    public long? ParentId { get; set; }
    public int Level { get; set; }

    [MaxLength(1000)] public string? Path { get; set; }

    // Classification
    [MaxLength(100)] public string? CategoryType { get; set; }

    [MaxLength(100)] public string? Industry { get; set; }

    [MaxLength(100)] public string? Sector { get; set; }

    // Accounting
    [MaxLength(50)] public string? GLAccountCode { get; set; }

    [MaxLength(50)] public string? DepreciationAccountCode { get; set; }

    [MaxLength(50)] public string? CostCenter { get; set; }

    // Depreciation Defaults
    [MaxLength(100)] public string? DefaultDepreciationMethod { get; set; }

    public int? DefaultUsefulLifeMonths { get; set; }
    public decimal? DefaultResidualValuePercentage { get; set; }

    // Properties
    public bool IsDepreciable { get; set; } = true;
    public bool IsCapitalized { get; set; } = true;
    public bool RequiresMaintenance { get; set; } = true;
    public bool RequiresInsurance { get; set; }

    // Valuation
    [Column(TypeName = "decimal(18,2)")] public decimal? MinimumCapitalizationValue { get; set; }

    [MaxLength(100)] public string? ValuationMethod { get; set; }

    // Compliance
    public bool RequiresCompliance { get; set; }
    public string? ComplianceStandards { get; set; }
    public bool RequiresAudit { get; set; }
    public int? AuditIntervalMonths { get; set; }

    // Display
    [MaxLength(100)] public string? IconName { get; set; }

    [MaxLength(500)] public string? IconUrl { get; set; }

    [MaxLength(20)] public string? Color { get; set; }

    public int DisplayOrder { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool IsSystemCategory { get; set; }

    // Tags & Search
    public string? Tags { get; set; }
    public string? SearchKeywords { get; set; }
    public string? Aliases { get; set; }

    // Statistics
    public int AssetCount { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal? TotalValue { get; set; }

    // Internal Notes
    public string? InternalNotes { get; set; }

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
    [ForeignKey("ParentId")] public AssetCategoryEf? Parent { get; set; }

    public ICollection<AssetCategoryEf> Children { get; set; } = new List<AssetCategoryEf>();
    public ICollection<ModelEf> Models { get; set; } = new List<ModelEf>();
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}
