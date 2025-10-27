using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FAM.Domain.Types;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for AssetType
/// </summary>
[BsonIgnoreExtraElements]
public class AssetTypeMongo : BaseEntityMongo
{
    // Basic Information
    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("longDescription")]
    public string? LongDescription { get; set; }

    // Hierarchy
    [BsonElement("parentId")]
    public long? ParentId { get; set; }

    [BsonElement("level")]
    public int Level { get; set; }

    [BsonElement("path")]
    public string? Path { get; set; }

    // Classification
    [BsonElement("assetClass")]
    public string? AssetClass { get; set; }

    [BsonElement("category")]
    public string? Category { get; set; }

    [BsonElement("subcategory")]
    public string? Subcategory { get; set; }

    // Properties & Characteristics
    [BsonElement("isDepreciable")]
    public bool IsDepreciable { get; set; } = true;

    [BsonElement("isAssignable")]
    public bool IsAssignable { get; set; } = true;

    [BsonElement("isTangible")]
    public bool IsTangible { get; set; } = true;

    [BsonElement("isConsumable")]
    public bool IsConsumable { get; set; }

    [BsonElement("isCapitalized")]
    public bool IsCapitalized { get; set; } = true;

    [BsonElement("requiresLicense")]
    public bool RequiresLicense { get; set; }

    [BsonElement("requiresMaintenance")]
    public bool RequiresMaintenance { get; set; } = true;

    [BsonElement("requiresCalibration")]
    public bool RequiresCalibration { get; set; }

    [BsonElement("requiresInsurance")]
    public bool RequiresInsurance { get; set; }

    [BsonElement("isITAsset")]
    public bool IsITAsset { get; set; }

    // Depreciation Defaults
    [BsonElement("defaultDepreciationMethod")]
    public string? DefaultDepreciationMethod { get; set; }

    [BsonElement("defaultUsefulLifeMonths")]
    public int? DefaultUsefulLifeMonths { get; set; }

    [BsonElement("defaultResidualValuePercentage")]
    public decimal? DefaultResidualValuePercentage { get; set; }

    [BsonElement("depreciationAccountCode")]
    public string? DepreciationAccountCode { get; set; }

    [BsonElement("accumulatedDepreciationAccountCode")]
    public string? AccumulatedDepreciationAccountCode { get; set; }

    // Accounting
    [BsonElement("glAccountCode")]
    public string? GLAccountCode { get; set; }

    [BsonElement("assetAccountCode")]
    public string? AssetAccountCode { get; set; }

    [BsonElement("expenseAccountCode")]
    public string? ExpenseAccountCode { get; set; }

    [BsonElement("costCenter")]
    public string? CostCenter { get; set; }

    // Lifecycle
    [BsonElement("defaultWarrantyMonths")]
    public int? DefaultWarrantyMonths { get; set; }

    [BsonElement("defaultMaintenanceIntervalDays")]
    public int? DefaultMaintenanceIntervalDays { get; set; }

    [BsonElement("defaultMaintenanceType")]
    public string? DefaultMaintenanceType { get; set; }

    // Valuation
    [BsonElement("minimumCapitalizationValue")]
    public decimal? MinimumCapitalizationValue { get; set; }

    [BsonElement("valuationCurrency")]
    public string? ValuationCurrency { get; set; }

    [BsonElement("valuationMethod")]
    public string? ValuationMethod { get; set; }

    // Compliance & Regulations
    [BsonElement("requiresCompliance")]
    public bool RequiresCompliance { get; set; }

    [BsonElement("complianceStandards")]
    public string? ComplianceStandards { get; set; }

    [BsonElement("regulatoryRequirements")]
    public string? RegulatoryRequirements { get; set; }

    [BsonElement("requiresAudit")]
    public bool RequiresAudit { get; set; }

    [BsonElement("auditIntervalMonths")]
    public int? AuditIntervalMonths { get; set; }

    // Security
    [BsonElement("defaultSecurityClassification")]
    public string? DefaultSecurityClassification { get; set; }

    [BsonElement("requiresBackgroundCheck")]
    public bool RequiresBackgroundCheck { get; set; }

    [BsonElement("requiresAccessControl")]
    public bool RequiresAccessControl { get; set; }

    // Workflow & Approval
    [BsonElement("requiresApprovalToAcquire")]
    public bool RequiresApprovalToAcquire { get; set; }

    [BsonElement("requiresApprovalToDispose")]
    public bool RequiresApprovalToDispose { get; set; }

    [BsonElement("approvalWorkflow")]
    public string? ApprovalWorkflow { get; set; }

    // Custom Fields Definition
    [BsonElement("customFieldsSchema")]
    public string? CustomFieldsSchema { get; set; }

    [BsonElement("requiredFields")]
    public string? RequiredFields { get; set; }

    // Display & UI
    [BsonElement("iconName")]
    public string? IconName { get; set; }

    [BsonElement("iconUrl")]
    public string? IconUrl { get; set; }

    [BsonElement("color")]
    public string? Color { get; set; }

    [BsonElement("displayOrder")]
    public int DisplayOrder { get; set; }

    // Status & Metadata
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("isSystemType")]
    public bool IsSystemType { get; set; }

    [BsonElement("effectiveDate")]
    public DateTime? EffectiveDate { get; set; }

    [BsonElement("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    // Tags & Search
    [BsonElement("tags")]
    public string? Tags { get; set; }

    [BsonElement("searchKeywords")]
    public string? SearchKeywords { get; set; }

    [BsonElement("aliases")]
    public string? Aliases { get; set; }

    // Statistics
    [BsonElement("assetCount")]
    public int AssetCount { get; set; }

    [BsonElement("totalValue")]
    public decimal? TotalValue { get; set; }

    // Internal Notes
    [BsonElement("internalNotes")]
    public string? InternalNotes { get; set; }

    [BsonElement("procurementNotes")]
    public string? ProcurementNotes { get; set; }

    // Navigation properties (NOT stored in MongoDB - use references instead)
    [BsonIgnore]
    public AssetTypeMongo? Parent { get; set; }

    [BsonIgnore]
    public List<AssetTypeMongo> Children { get; set; } = new();

    // Reference IDs for related entities (stored)
    [BsonElement("modelIds")]
    [BsonIgnoreIfNull]
    public List<long>? ModelIds { get; set; }

    [BsonElement("assetIds")]
    [BsonIgnoreIfNull]
    public List<long>? AssetIds { get; set; }

    public AssetTypeMongo() { }

    public AssetTypeMongo(long domainId) : base(domainId) { }
}