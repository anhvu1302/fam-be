using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Models;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Types;

/// <summary>
/// Loại tài sản - Theo chuẩn phân loại tài sản doanh nghiệp
/// </summary>
public class AssetType : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    // Basic Information
    public string Code { get; private set; } = string.Empty; // IT, OA, VE, RE, etc.
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LongDescription { get; private set; }

    // Hierarchy
    public long? ParentId { get; private set; } // For hierarchical types
    public int Level { get; private set; } // Hierarchy level (0 = root)
    public string? Path { get; private set; } // Full path (e.g., "/IT/Hardware/Laptop")

    // Classification
    public string? AssetClass { get; private set; } // Fixed Asset, Current Asset, Intangible
    public string? Category { get; private set; } // Primary category
    public string? Subcategory { get; private set; }

    // Properties & Characteristics
    public bool IsDepreciable { get; private set; } = true;
    public bool IsAssignable { get; private set; } = true; // Can be assigned to users
    public bool IsTangible { get; private set; } = true; // Physical vs Intangible
    public bool IsConsumable { get; private set; } // Consumable items
    public bool IsCapitalized { get; private set; } = true; // Capitalizable asset
    public bool RequiresLicense { get; private set; } // Software, patents, etc.
    public bool RequiresMaintenance { get; private set; } = true;
    public bool RequiresCalibration { get; private set; } // Measurement equipment
    public bool RequiresInsurance { get; private set; }
    public bool IsITAsset { get; private set; }

    // Depreciation Defaults
    public string? DefaultDepreciationMethod { get; private set; } // Straight Line, Declining Balance
    public int? DefaultUsefulLifeMonths { get; private set; }
    public decimal? DefaultResidualValuePercentage { get; private set; }
    public string? DepreciationAccountCode { get; private set; }
    public string? AccumulatedDepreciationAccountCode { get; private set; }

    // Accounting
    public string? GLAccountCode { get; private set; } // General Ledger account
    public string? AssetAccountCode { get; private set; }
    public string? ExpenseAccountCode { get; private set; }
    public string? CostCenter { get; private set; } // Default cost center

    // Lifecycle
    public int? DefaultWarrantyMonths { get; private set; }
    public int? DefaultMaintenanceIntervalDays { get; private set; }
    public string? DefaultMaintenanceType { get; private set; }

    // Valuation
    public decimal? MinimumCapitalizationValue { get; private set; } // Minimum value to capitalize
    public string? ValuationCurrency { get; private set; }
    public string? ValuationMethod { get; private set; } // Cost, Market, Book Value

    // Compliance & Regulations
    public bool RequiresCompliance { get; private set; }
    public string? ComplianceStandards { get; private set; } // JSON array (HIPAA, SOX, GDPR, etc.)
    public string? RegulatoryRequirements { get; private set; }
    public bool RequiresAudit { get; private set; }
    public int? AuditIntervalMonths { get; private set; }

    // Security
    public string? DefaultSecurityClassification { get; private set; } // Public, Internal, Confidential
    public bool RequiresBackgroundCheck { get; private set; }
    public bool RequiresAccessControl { get; private set; }

    // Workflow & Approval
    public bool RequiresApprovalToAcquire { get; private set; }
    public bool RequiresApprovalToDispose { get; private set; }
    public string? ApprovalWorkflow { get; private set; } // JSON workflow definition

    // Custom Fields Definition
    public string? CustomFieldsSchema { get; private set; } // JSON schema for custom fields
    public string? RequiredFields { get; private set; } // JSON array of required field names

    // Display & UI
    public string? IconName { get; private set; } // Icon identifier
    public string? IconUrl { get; private set; }
    public string? Color { get; private set; } // Hex color for UI
    public int DisplayOrder { get; private set; }

    // Status & Metadata
    public bool IsActive { get; private set; } = true;
    public bool IsSystemType { get; private set; } // System-defined, cannot be deleted
    public DateTime? EffectiveDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    // Tags & Search
    public string? Tags { get; private set; } // JSON array
    public string? SearchKeywords { get; private set; }
    public string? Aliases { get; private set; } // JSON array of alternative names

    // Statistics (can be calculated)
    public int AssetCount { get; private set; } // Current count
    public decimal? TotalValue { get; private set; } // Total value of assets

    // Internal Notes
    public string? InternalNotes { get; private set; }
    public string? ProcurementNotes { get; private set; }

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
    public AssetType? Parent { get; set; }
    public ICollection<AssetType> Children { get; set; } = new List<AssetType>();
    public ICollection<Model> Models { get; set; } = new List<Model>();

    private AssetType()
    {
    }

    public static AssetType Create(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Asset type code is required");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Asset type name is required");
        }

        return new AssetType
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            Level = 0,
            IsActive = true,
            IsDepreciable = true,
            IsAssignable = true,
            IsTangible = true,
            IsCapitalized = true,
            RequiresMaintenance = true
        };
    }

    public void UpdateBasicInfo(
        string name,
        string? description,
        string? longDescription)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Asset type name is required");
        }

        Name = name;
        Description = description;
        LongDescription = longDescription;
    }

    public void UpdateClassification(
        string? assetClass,
        string? category,
        string? subcategory)
    {
        AssetClass = assetClass;
        Category = category;
        Subcategory = subcategory;
    }

    public void UpdateProperties(
        bool isDepreciable,
        bool isAssignable,
        bool isTangible,
        bool isConsumable,
        bool isCapitalized,
        bool requiresLicense,
        bool requiresMaintenance,
        bool requiresCalibration,
        bool requiresInsurance,
        bool isITAsset)
    {
        IsDepreciable = isDepreciable;
        IsAssignable = isAssignable;
        IsTangible = isTangible;
        IsConsumable = isConsumable;
        IsCapitalized = isCapitalized;
        RequiresLicense = requiresLicense;
        RequiresMaintenance = requiresMaintenance;
        RequiresCalibration = requiresCalibration;
        RequiresInsurance = requiresInsurance;
        IsITAsset = isITAsset;
    }

    public void UpdateDepreciationDefaults(
        string? method,
        int? usefulLifeMonths,
        decimal? residualValuePercentage,
        string? depreciationAccountCode,
        string? accumulatedDepreciationAccountCode)
    {
        if (usefulLifeMonths.HasValue && usefulLifeMonths.Value < 0)
        {
            throw new DomainException("Default useful life months cannot be negative");
        }

        if (residualValuePercentage.HasValue &&
            (residualValuePercentage.Value < 0 || residualValuePercentage.Value > 100))
        {
            throw new DomainException("Default residual value percentage must be between 0 and 100");
        }

        DefaultDepreciationMethod = method;
        DefaultUsefulLifeMonths = usefulLifeMonths;
        DefaultResidualValuePercentage = residualValuePercentage;
        DepreciationAccountCode = depreciationAccountCode;
        AccumulatedDepreciationAccountCode = accumulatedDepreciationAccountCode;
    }

    public void UpdateAccounting(
        string? glAccountCode,
        string? assetAccountCode,
        string? expenseAccountCode,
        string? costCenter)
    {
        GLAccountCode = glAccountCode;
        AssetAccountCode = assetAccountCode;
        ExpenseAccountCode = expenseAccountCode;
        CostCenter = costCenter;
    }

    public void UpdateLifecycleDefaults(
        int? warrantyMonths,
        int? maintenanceIntervalDays,
        string? maintenanceType)
    {
        if (warrantyMonths.HasValue && warrantyMonths.Value < 0)
        {
            throw new DomainException("Default warranty months cannot be negative");
        }

        if (maintenanceIntervalDays.HasValue && maintenanceIntervalDays.Value < 0)
        {
            throw new DomainException("Default maintenance interval days cannot be negative");
        }

        DefaultWarrantyMonths = warrantyMonths;
        DefaultMaintenanceIntervalDays = maintenanceIntervalDays;
        DefaultMaintenanceType = maintenanceType;
    }

    public void UpdateValuation(
        decimal? minCapitalizationValue,
        string? currency,
        string? valuationMethod)
    {
        if (minCapitalizationValue.HasValue && minCapitalizationValue.Value < 0)
        {
            throw new DomainException("Minimum capitalization value cannot be negative");
        }

        MinimumCapitalizationValue = minCapitalizationValue;
        ValuationCurrency = currency;
        ValuationMethod = valuationMethod;
    }

    public void UpdateCompliance(
        bool requiresCompliance,
        string? complianceStandards,
        string? regulatoryRequirements,
        bool requiresAudit,
        int? auditIntervalMonths)
    {
        if (auditIntervalMonths.HasValue && auditIntervalMonths.Value < 0)
        {
            throw new DomainException("Audit interval months cannot be negative");
        }

        RequiresCompliance = requiresCompliance;
        ComplianceStandards = complianceStandards;
        RegulatoryRequirements = regulatoryRequirements;
        RequiresAudit = requiresAudit;
        AuditIntervalMonths = auditIntervalMonths;
    }

    public void UpdateSecurity(
        string? defaultSecurityClassification,
        bool requiresBackgroundCheck,
        bool requiresAccessControl)
    {
        DefaultSecurityClassification = defaultSecurityClassification;
        RequiresBackgroundCheck = requiresBackgroundCheck;
        RequiresAccessControl = requiresAccessControl;
    }

    public void UpdateWorkflow(
        bool requiresApprovalToAcquire,
        bool requiresApprovalToDispose,
        string? approvalWorkflow)
    {
        RequiresApprovalToAcquire = requiresApprovalToAcquire;
        RequiresApprovalToDispose = requiresApprovalToDispose;
        ApprovalWorkflow = approvalWorkflow;
    }

    public void UpdateCustomFields(
        string? customFieldsSchema,
        string? requiredFields)
    {
        CustomFieldsSchema = customFieldsSchema;
        RequiredFields = requiredFields;
    }

    public void UpdateDisplay(
        string? iconName,
        string? iconUrl,
        string? color,
        int displayOrder)
    {
        IconName = iconName;
        IconUrl = ValidateUrl(iconUrl);
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

    public void MarkAsSystemType()
    {
        IsSystemType = true;
    }

    public void UpdateStatistics(int assetCount, decimal? totalValue)
    {
        if (assetCount < 0)
        {
            throw new DomainException("Asset count cannot be negative");
        }

        if (totalValue.HasValue && totalValue.Value < 0)
        {
            throw new DomainException("Total value cannot be negative");
        }

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

    // Private helper methods
    private string? ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        Url urlVo = Url.Create(url);
        return urlVo.Value;
    }
}
