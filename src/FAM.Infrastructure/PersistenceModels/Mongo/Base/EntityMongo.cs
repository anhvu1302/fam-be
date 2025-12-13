using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo.Base;

/// <summary>
/// EntityMongo - Simple entity with creation tracking only
/// Properties: Id, CreatedAt, CreatedById
/// </summary>
public abstract class EntityMongo : BaseEntityMongo
{
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdById")] public long? CreatedById { get; set; }

    protected EntityMongo() : base()
    {
    }

    protected EntityMongo(long domainId) : base(domainId)
    {
    }
}
