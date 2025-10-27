using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FAM.Domain.Categories;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for AssetCategory
/// </summary>
[BsonIgnoreExtraElements]
public class AssetCategoryMongo : BaseEntityMongo
{
    // Basic Information
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("code")]
    public string? Code { get; set; }

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
    [BsonElement("categoryType")]
    public string? CategoryType { get; set; }

    [BsonElement("industry")]
    public string? Industry { get; set; }

    [BsonElement("sector")]
    public string? Sector { get; set; }

    // Accounting
    [BsonElement("glAccountCode")]
    public string? GLAccountCode { get; set; }

    [BsonElement("depreciationAccountCode")]
    public string? DepreciationAccountCode { get; set; }

    [BsonElement("costCenter")]
    public string? CostCenter { get; set; }

    // Depreciation Defaults
    [BsonElement("defaultDepreciationMethod")]
    public string? DefaultDepreciationMethod { get; set; }

    [BsonElement("defaultUsefulLifeMonths")]
    public int? DefaultUsefulLifeMonths { get; set; }

    [BsonElement("defaultResidualValuePercentage")]
    public decimal? DefaultResidualValuePercentage { get; set; }

    // Properties
    [BsonElement("isDepreciable")]
    public bool IsDepreciable { get; set; } = true;

    [BsonElement("isCapitalized")]
    public bool IsCapitalized { get; set; } = true;

    [BsonElement("requiresMaintenance")]
    public bool RequiresMaintenance { get; set; } = true;

    [BsonElement("requiresInsurance")]
    public bool RequiresInsurance { get; set; }

    // Valuation
    [BsonElement("minimumCapitalizationValue")]
    public decimal? MinimumCapitalizationValue { get; set; }

    [BsonElement("valuationMethod")]
    public string? ValuationMethod { get; set; }

    // Compliance
    [BsonElement("requiresCompliance")]
    public bool RequiresCompliance { get; set; }

    [BsonElement("complianceStandards")]
    public string? ComplianceStandards { get; set; }

    [BsonElement("requiresAudit")]
    public bool RequiresAudit { get; set; }

    [BsonElement("auditIntervalMonths")]
    public int? AuditIntervalMonths { get; set; }

    // Display
    [BsonElement("iconName")]
    public string? IconName { get; set; }

    [BsonElement("iconUrl")]
    public string? IconUrl { get; set; }

    [BsonElement("color")]
    public string? Color { get; set; }

    [BsonElement("displayOrder")]
    public int DisplayOrder { get; set; }

    // Status
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("isSystemCategory")]
    public bool IsSystemCategory { get; set; }

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

    // Navigation properties (stored as references)
    [BsonElement("parent")]
    public AssetCategoryMongo? Parent { get; set; }

    [BsonElement("children")]
    public List<AssetCategoryMongo> Children { get; set; } = new();

    [BsonElement("modelIds")]
    public List<long> ModelIds { get; set; } = new();

    [BsonElement("assetIds")]
    public List<long> AssetIds { get; set; } = new();

    public AssetCategoryMongo() { }

    public AssetCategoryMongo(long domainId) : base(domainId) { }
}