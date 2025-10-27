using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FAM.Domain.Statuses;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for UsageStatus
/// </summary>
[BsonIgnoreExtraElements]
public class UsageStatusMongo : BaseEntityMongo
{
    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("color")]
    public string? Color { get; set; }

    [BsonElement("orderNo")]
    public int? OrderNo { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetIds")]
    public List<long> AssetIds { get; set; } = new();

    public UsageStatusMongo() { }

    public UsageStatusMongo(long domainId) : base(domainId) { }
}