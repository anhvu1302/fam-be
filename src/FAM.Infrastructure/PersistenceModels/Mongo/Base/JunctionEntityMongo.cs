using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo.Base;

/// <summary>
/// Base class for MongoDB junction documents (many-to-many relationships)
/// Junction documents use composite keys and minimal audit data
/// Only tracks creation timestamp for audit trail
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