using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo.Base;

/// <summary>
/// Base entity for MongoDB documents with generic TId for domain ID
/// Contains ONLY MongoDB Id and Domain Id - mirrors domain BaseEntity<TId> structure
/// </summary>
public abstract class BaseEntityMongo<TId> where TId : struct
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Domain ID mapping (for cross-references with relational DB)
    [BsonElement("domainId")] public TId DomainId { get; set; }

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