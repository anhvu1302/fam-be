using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// Base entity for MongoDB documents using GUID as domain ID
/// </summary>
public abstract class BaseEntityMongoGuid
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Domain ID mapping (GUID for high-volume tables)
    [BsonElement("domainId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid DomainId { get; set; }

    // Audit fields (timestamps)
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    // Soft delete
    [BsonElement("isDeleted")] public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")] public DateTime? DeletedAt { get; set; }

    protected BaseEntityMongoGuid()
    {
        DomainId = Guid.NewGuid();
    }

    protected BaseEntityMongoGuid(Guid domainId)
    {
        DomainId = domainId;
    }
}