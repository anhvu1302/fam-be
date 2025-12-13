using FAM.Infrastructure.PersistenceModels.Mongo.Base;

using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for AssetEventType
/// </summary>
[BsonIgnoreExtraElements]
public class AssetEventTypeMongo : BaseEntityMongo
{
    [BsonElement("code")] public string Code { get; set; } = string.Empty;

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("description")] public string? Description { get; set; }

    [BsonElement("color")] public string? Color { get; set; }

    [BsonElement("orderNo")] public int? OrderNo { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetEventIds")] public List<long> AssetEventIds { get; set; } = new();

    public AssetEventTypeMongo()
    {
    }

    public AssetEventTypeMongo(long domainId) : base(domainId)
    {
    }
}
