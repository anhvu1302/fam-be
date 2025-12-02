using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for Model
/// </summary>
[BsonIgnoreExtraElements]
public class ModelMongo : BaseEntityMongo
{
    // Basic Information
    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("modelNumber")] public string? ModelNumber { get; set; }

    [BsonElement("sku")] public string? SKU { get; set; }

    [BsonElement("partNumber")] public string? PartNumber { get; set; }

    [BsonElement("description")] public string? Description { get; set; }

    // Relationships
    [BsonElement("manufacturerId")] public long? ManufacturerId { get; set; }

    [BsonElement("categoryId")] public long? CategoryId { get; set; }

    [BsonElement("typeId")] public long? TypeId { get; set; }

    // Product Information
    [BsonElement("productFamily")] public string? ProductFamily { get; set; }

    [BsonElement("generation")] public string? Generation { get; set; }

    [BsonElement("series")] public string? Series { get; set; }

    [BsonElement("releaseDate")] public DateTime? ReleaseDate { get; set; }

    [BsonElement("discontinuedDate")] public DateTime? DiscontinuedDate { get; set; }

    [BsonElement("lifecycleStatus")] public string? LifecycleStatus { get; set; }

    // Technical Specifications
    [BsonElement("technicalSpecs")] public string? TechnicalSpecs { get; set; }

    [BsonElement("processor")] public string? Processor { get; set; }

    [BsonElement("memory")] public string? Memory { get; set; }

    [BsonElement("storage")] public string? Storage { get; set; }

    [BsonElement("display")] public string? Display { get; set; }

    [BsonElement("graphics")] public string? Graphics { get; set; }

    [BsonElement("operatingSystem")] public string? OperatingSystem { get; set; }

    // Physical Specifications
    [BsonElement("weight")] public decimal? Weight { get; set; }

    [BsonElement("weightUnit")] public string? WeightUnit { get; set; }

    [BsonElement("dimensions")] public string? Dimensions { get; set; }

    [BsonElement("dimensionUnit")] public string? DimensionUnit { get; set; }

    [BsonElement("color")] public string? Color { get; set; }

    [BsonElement("material")] public string? Material { get; set; }

    // Power & Environmental
    [BsonElement("powerRequirements")] public string? PowerRequirements { get; set; }

    [BsonElement("powerConsumption")] public decimal? PowerConsumption { get; set; }

    [BsonElement("energyRating")] public string? EnergyRating { get; set; }

    // Warranty & Support
    [BsonElement("warrantyMonths")] public int? WarrantyMonths { get; set; }

    [BsonElement("warrantyType")] public string? WarrantyType { get; set; }

    [BsonElement("supportEndDate")] public DateTime? SupportEndDate { get; set; }

    // Pricing
    [BsonElement("msrp")] public decimal? MSRP { get; set; }

    [BsonElement("currency")] public string? Currency { get; set; }

    // Status
    [BsonElement("isActive")] public bool IsActive { get; set; } = true;

    [BsonElement("isDiscontinued")] public bool IsDiscontinued { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetIds")] public List<long> AssetIds { get; set; } = new();

    public ModelMongo()
    {
    }

    public ModelMongo(long domainId) : base(domainId)
    {
    }
}