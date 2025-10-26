using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// Entity with audit user tracking for MongoDB
/// </summary>
public abstract class EntityMongo : BaseEntityMongo
{
    // Audit user IDs (stored in database)
    [BsonElement("createdById")]
    public long? CreatedById { get; set; }

    [BsonElement("updatedById")]
    public long? UpdatedById { get; set; }

    [BsonElement("deletedById")]
    public long? DeletedById { get; set; }

    // Navigation properties (not stored, populated manually if needed)
    [BsonIgnore]
    public UserMongo? CreatedBy { get; set; }

    [BsonIgnore]
    public UserMongo? UpdatedBy { get; set; }

    [BsonIgnore]
    public UserMongo? DeletedBy { get; set; }

    protected EntityMongo() : base() { }

    protected EntityMongo(long domainId) : base(domainId) { }
}
