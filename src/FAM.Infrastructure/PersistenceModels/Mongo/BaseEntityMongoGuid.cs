using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// Base entity for MongoDB documents using GUID as domain ID
/// Inherits all audit fields from BaseEntityMongo<Guid>
/// </summary>
public abstract class BaseEntityMongoGuid : BaseEntityMongo<Guid>
{
    // Override DomainId to add GUID-specific serialization
    [BsonElement("domainId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public new Guid DomainId { get; set; }

    protected BaseEntityMongoGuid()
    {
        DomainId = Guid.NewGuid();
    }

    protected BaseEntityMongoGuid(Guid domainId) : base(domainId)
    {
        DomainId = domainId;
    }
}