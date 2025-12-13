using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo.Base;

/// <summary>
/// FullAuditedEntityMongo - Entity with complete audit tracking (WHO + WHEN)
/// Properties: Id, CreatedAt, CreatedById, UpdatedAt, UpdatedById, IsDeleted, DeletedAt, DeletedById
/// </summary>
public abstract class FullAuditedEntityMongo : BaseEntityMongo
{
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdById")] public long? CreatedById { get; set; }

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    [BsonElement("updatedById")] public long? UpdatedById { get; set; }

    [BsonElement("isDeleted")] public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")] public DateTime? DeletedAt { get; set; }

    [BsonElement("deletedById")] public long? DeletedById { get; set; }

    // Navigation properties (not stored, populated manually if needed)
    [BsonIgnore] public UserMongo? UpdatedBy { get; set; }

    [BsonIgnore] public UserMongo? DeletedBy { get; set; }

    protected FullAuditedEntityMongo() : base()
    {
    }

    protected FullAuditedEntityMongo(long domainId) : base(domainId)
    {
    }
}
