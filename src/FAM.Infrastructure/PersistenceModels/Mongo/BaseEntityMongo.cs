using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// Base class for MongoDB junction documents (many-to-many relationships)
/// Junction documents use composite keys and minimal audit data
/// </summary>
public abstract class JunctionEntityMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected JunctionEntityMongo()
    {
    }
}

/// <summary>
/// Base entity for MongoDB documents with generic TId for domain ID
/// Mirrors domain BaseEntity<TId> structure
/// </summary>
public abstract class BaseEntityMongo<TId> where TId : struct
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Domain ID mapping (for cross-references with relational DB)
    [BsonElement("domainId")] public TId DomainId { get; set; }

    // Audit fields - sync with domain BaseEntity
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdById")] public long? CreatedById { get; set; }

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    [BsonElement("updatedById")] public long? UpdatedById { get; set; }

    // Soft delete
    [BsonElement("isDeleted")] public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")] public DateTime? DeletedAt { get; set; }

    [BsonElement("deletedById")] public long? DeletedById { get; set; }

    protected BaseEntityMongo()
    {
    }

    protected BaseEntityMongo(TId domainId)
    {
        DomainId = domainId;
    }
}

/// <summary>
/// Base entity with long domain ID (most common case)
/// </summary>
public abstract class BaseEntityMongo : BaseEntityMongo<long>
{
    protected BaseEntityMongo() : base()
    {
    }

    protected BaseEntityMongo(long domainId) : base(domainId)
    {
    }
}