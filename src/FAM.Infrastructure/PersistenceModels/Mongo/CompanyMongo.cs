using FAM.Infrastructure.PersistenceModels.Mongo.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for Company
/// </summary>
[BsonCollection("companies")]
public class CompanyMongo : EntityMongo
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("taxCode")] public string? TaxCode { get; set; }

    [BsonElement("address")] public string? Address { get; set; }

    public CompanyMongo() : base()
    {
    }

    public CompanyMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// Attribute to specify collection name for MongoDB
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BsonCollectionAttribute : Attribute
{
    public string CollectionName { get; }

    public BsonCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }
}