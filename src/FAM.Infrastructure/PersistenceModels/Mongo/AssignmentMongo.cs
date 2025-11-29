using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FAM.Domain.Assets;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for Assignment
/// </summary>
[BsonIgnoreExtraElements]
public class AssignmentMongo : BaseEntityMongo
{
    [BsonElement("assetId")] public long AssetId { get; set; }

    [BsonElement("assigneeType")] public string AssigneeType { get; set; } = string.Empty;

    [BsonElement("assigneeId")] public long AssigneeId { get; set; }

    [BsonElement("assignedAt")] public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("releasedAt")] public DateTime? ReleasedAt { get; set; }

    [BsonElement("byUserId")] public long? ByUserId { get; set; }

    [BsonElement("comments")] public string? Comments { get; set; }

    public AssignmentMongo()
    {
    }

    public AssignmentMongo(long domainId) : base(domainId)
    {
    }
}