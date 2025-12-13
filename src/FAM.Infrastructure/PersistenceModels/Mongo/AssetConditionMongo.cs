using FAM.Infrastructure.PersistenceModels.Mongo.Base;

using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for AssetCondition
/// </summary>
[BsonIgnoreExtraElements]
public class AssetConditionMongo : BaseEntityMongo
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("description")] public string? Description { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetIds")] public List<long> AssetIds { get; set; } = new();

    public AssetConditionMongo()
    {
    }

    public AssetConditionMongo(long domainId) : base(domainId)
    {
    }
}
