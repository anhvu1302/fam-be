using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FAM.Domain.Assets;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for Attachment
/// </summary>
[BsonIgnoreExtraElements]
public class AttachmentMongo : BaseEntityMongo
{
    [BsonElement("assetId")] public long? AssetId { get; set; }

    [BsonElement("fileName")] public string? FileName { get; set; }

    [BsonElement("fileUrl")] public string? FileUrl { get; set; }

    [BsonElement("uploadedBy")] public long? UploadedBy { get; set; }

    [BsonElement("uploadedAt")] public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public AttachmentMongo()
    {
    }

    public AttachmentMongo(long domainId) : base(domainId)
    {
    }
}