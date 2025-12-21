using FAM.Domain.Assets;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Models;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Categories;

/// <summary>
/// Danh mục tài sản - Phân loại theo ngành, mục đích sử dụng
/// </summary>
public class AssetCategory : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    // Basic Information
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; } // Category code
    public string? Description { get; private set; }
    public string? LongDescription { get; private set; }

    // Hierarchy
    public long? ParentId { get; private set; }
    public int Level { get; private set; }
    public string? Path { get; private set; } // Full hierarchical path

    // Classification
    public string? CategoryType { get; private set; } // Functional, Departmental, Industry-specific
    public string? Industry { get; private set; } // Healthcare, Manufacturing, Retail, etc.
    public string? Sector { get; private set; }

    // Accounting
    public string? GLAccountCode { get; private set; }
    public string? DepreciationAccountCode { get; private set; }
    public string? CostCenter { get; private set; }

    // Depreciation Defaults
    public string? DefaultDepreciationMethod { get; private set; }
    public int? DefaultUsefulLifeMonths { get; private set; }
    public decimal? DefaultResidualValuePercentage { get; private set; }

    // Properties
    public bool IsDepreciable { get; private set; } = true;
    public bool IsCapitalized { get; private set; } = true;
    public bool RequiresMaintenance { get; private set; } = true;
    public bool RequiresInsurance { get; private set; }

    // Valuation
    public decimal? MinimumCapitalizationValue { get; private set; }
    public string? ValuationMethod { get; private set; }

    // Compliance
    public bool RequiresCompliance { get; private set; }
    public string? ComplianceStandards { get; private set; } // JSON array
    public bool RequiresAudit { get; private set; }
    public int? AuditIntervalMonths { get; private set; }

    // Display
    public string? IconName { get; private set; }
    public Url? IconUrl { get; private set; }
    public string? Color { get; private set; }
    public int DisplayOrder { get; private set; }

    // Status
    public bool IsActive { get; private set; } = true;
    public bool IsSystemCategory { get; private set; }

    // Tags & Search
    public string? Tags { get; private set; } // JSON array
    public string? SearchKeywords { get; private set; }
    public string? Aliases { get; private set; }

    // Statistics
    public int AssetCount { get; private set; }
    public decimal? TotalValue { get; private set; }

    // Internal Notes
    public string? InternalNotes { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
    public User? DeletedBy { get; set; }
    public AssetCategory? Parent { get; set; }
    public ICollection<AssetCategory> Children { get; set; } = new List<AssetCategory>();
    public ICollection<Model> Models { get; set; } = new List<Model>();

    private AssetCategory()
    {
    }

    public static AssetCategory Create(string name, string? code = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required");

        return new AssetCategory
        {
            Name = name,
            Code = code?.ToUpperInvariant(),
            Description = description,
            Level = 0,
            IsActive = true,
            IsDepreciable = true,
            IsCapitalized = true,
            RequiresMaintenance = true
        };
    }

    public void UpdateBasicInfo(
        string name,
        string? code,
        string? description,
        string? longDescription)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required");

        Name = name;
        Code = code?.ToUpperInvariant();
        Description = description;
        LongDescription = longDescription;
    }

    public void UpdateClassification(
        string? categoryType,
        string? industry,
        string? sector)
    {
        CategoryType = categoryType;
        Industry = industry;
        Sector = sector;
    }

    public void UpdateAccounting(
        string? glAccountCode,
        string? depreciationAccountCode,
        string? costCenter)
    {
        GLAccountCode = glAccountCode;
        DepreciationAccountCode = depreciationAccountCode;
        CostCenter = costCenter;
    }

    public void UpdateDepreciationDefaults(
        string? method,
        int? usefulLifeMonths,
        decimal? residualValuePercentage)
    {
        if (usefulLifeMonths.HasValue && usefulLifeMonths.Value < 0)
            throw new DomainException("Default useful life months cannot be negative");

        if (residualValuePercentage.HasValue &&
            (residualValuePercentage.Value < 0 || residualValuePercentage.Value > 100))
            throw new DomainException("Default residual value percentage must be between 0 and 100");

        DefaultDepreciationMethod = method;
        DefaultUsefulLifeMonths = usefulLifeMonths;
        DefaultResidualValuePercentage = residualValuePercentage;
    }

    public void UpdateProperties(
        bool isDepreciable,
        bool isCapitalized,
        bool requiresMaintenance,
        bool requiresInsurance)
    {
        IsDepreciable = isDepreciable;
        IsCapitalized = isCapitalized;
        RequiresMaintenance = requiresMaintenance;
        RequiresInsurance = requiresInsurance;
    }

    public void UpdateValuation(
        decimal? minCapitalizationValue,
        string? valuationMethod)
    {
        if (minCapitalizationValue.HasValue && minCapitalizationValue.Value < 0)
            throw new DomainException("Minimum capitalization value cannot be negative");

        MinimumCapitalizationValue = minCapitalizationValue;
        ValuationMethod = valuationMethod;
    }

    public void UpdateCompliance(
        bool requiresCompliance,
        string? complianceStandards,
        bool requiresAudit,
        int? auditIntervalMonths)
    {
        if (auditIntervalMonths.HasValue && auditIntervalMonths.Value < 0)
            throw new DomainException("Audit interval months cannot be negative");

        RequiresCompliance = requiresCompliance;
        ComplianceStandards = complianceStandards;
        RequiresAudit = requiresAudit;
        AuditIntervalMonths = auditIntervalMonths;
    }

    public void UpdateDisplay(
        string? iconName,
        string? iconUrl,
        string? color,
        int displayOrder)
    {
        IconName = iconName;
        IconUrl = iconUrl != null ? Url.Create(iconUrl) : null;
        Color = color;
        DisplayOrder = displayOrder;
    }

    public void SetParent(int? parentId, int level, string path)
    {
        ParentId = parentId;
        Level = level;
        Path = path;
    }

    public void UpdateSearchTags(
        string? tags,
        string? searchKeywords,
        string? aliases)
    {
        Tags = tags;
        SearchKeywords = searchKeywords;
        Aliases = aliases;
    }

    public void AddNotes(string? internalNotes)
    {
        InternalNotes = internalNotes;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void MarkAsSystemCategory()
    {
        IsSystemCategory = true;
    }

    public void UpdateStatistics(int assetCount, decimal? totalValue)
    {
        if (assetCount < 0)
            throw new DomainException("Asset count cannot be negative");

        if (totalValue.HasValue && totalValue.Value < 0)
            throw new DomainException("Total value cannot be negative");

        AssetCount = assetCount;
        TotalValue = totalValue;
    }

    public bool IsHierarchical()
    {
        return ParentId.HasValue || Children.Any();
    }

    public bool IsRoot()
    {
        return !ParentId.HasValue;
    }

    public bool IsLeaf()
    {
        return !Children.Any();
    }

    public void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
