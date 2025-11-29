using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// Base entity for MongoDB documents
/// </summary>
public abstract class BaseEntityMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Domain ID mapping (for cross-references with relational DB)
    [BsonElement("domainId")] public long DomainId { get; set; }

    // Audit fields (timestamps)
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    // Soft delete
    [BsonElement("isDeleted")] public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")] public DateTime? DeletedAt { get; set; }

    protected BaseEntityMongo()
    {
    }

    protected BaseEntityMongo(long domainId)
    {
        DomainId = domainId;
    }
}