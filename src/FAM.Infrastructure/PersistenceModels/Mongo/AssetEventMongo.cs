using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FAM.Domain.Assets;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for AssetEvent
/// </summary>
[BsonIgnoreExtraElements]
public class AssetEventMongo : BaseEntityMongo
{
    [BsonElement("assetId")] public long AssetId { get; set; }

    [BsonElement("eventCode")] public string EventCode { get; set; } = string.Empty;

    [BsonElement("actorId")] public long? ActorId { get; set; }

    [BsonElement("at")] public DateTime At { get; set; } = DateTime.UtcNow;

    [BsonElement("fromLifecycleCode")] public string? FromLifecycleCode { get; set; }

    [BsonElement("toLifecycleCode")] public string? ToLifecycleCode { get; set; }

    [BsonElement("payload")] public string? Payload { get; set; }

    [BsonElement("note")] public string? Note { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetEventTypeId")] public long? AssetEventTypeId { get; set; }

    public AssetEventMongo()
    {
    }

    public AssetEventMongo(long domainId) : base(domainId)
    {
    }
}