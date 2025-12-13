using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo.Base;

/// <summary>
/// BasicAuditedEntityMongo - Entity with basic audit tracking (WHEN only)
/// Properties: Id, CreatedAt, CreatedById, UpdatedAt, IsDeleted, DeletedAt
/// Does NOT have: UpdatedById, DeletedById
/// </summary>
public abstract class BasicAuditedEntityMongo : BaseEntityMongo
{
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdById")] public long? CreatedById { get; set; }

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    [BsonElement("isDeleted")] public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")] public DateTime? DeletedAt { get; set; }

    protected BasicAuditedEntityMongo() : base()
    {
    }

    protected BasicAuditedEntityMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// BasicAuditedEntity with GUID domain ID for MongoDB
/// </summary>
public abstract class BasicAuditedEntityMongoGuid : BaseEntityMongo<Guid>
{
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdById")] public long? CreatedById { get; set; }

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    [BsonElement("isDeleted")] public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")] public DateTime? DeletedAt { get; set; }

    protected BasicAuditedEntityMongoGuid() : base()
    {
        DomainId = Guid.NewGuid();
    }

    protected BasicAuditedEntityMongoGuid(Guid domainId) : base(domainId)
    {
    }
}
