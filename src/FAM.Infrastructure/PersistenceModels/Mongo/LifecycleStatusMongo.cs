using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for LifecycleStatus
/// </summary>
[BsonIgnoreExtraElements]
public class LifecycleStatusMongo : BaseEntityMongo
{
    [BsonElement("code")] public string Code { get; set; } = string.Empty;

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("description")] public string? Description { get; set; }

    [BsonElement("color")] public string? Color { get; set; }

    [BsonElement("orderNo")] public int? OrderNo { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetIds")] public List<long> AssetIds { get; set; } = new();

    public LifecycleStatusMongo()
    {
    }

    public LifecycleStatusMongo(long domainId) : base(domainId)
    {
    }
}