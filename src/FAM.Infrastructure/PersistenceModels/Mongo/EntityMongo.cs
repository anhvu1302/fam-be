using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// Entity with audit user tracking for MongoDB
/// Audit fields (CreatedById, UpdatedById, DeletedById) inherited from BaseEntityMongo
/// </summary>
public abstract class EntityMongo : BaseEntityMongo
{
    // Navigation properties (not stored, populated manually if needed)
    [BsonIgnore] public UserMongo? CreatedBy { get; set; }

    [BsonIgnore] public UserMongo? UpdatedBy { get; set; }

    [BsonIgnore] public UserMongo? DeletedBy { get; set; }

    protected EntityMongo() : base()
    {
    }

    protected EntityMongo(long domainId) : base(domainId)
    {
    }
}