using FAM.Infrastructure.PersistenceModels.Mongo.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for Location
/// </summary>
[BsonIgnoreExtraElements]
public class LocationMongo : BaseEntityMongo
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("companyId")] public long? CompanyId { get; set; }

    [BsonElement("code")] public string? Code { get; set; }

    [BsonElement("parentId")] public long? ParentId { get; set; }

    [BsonElement("type")] public string? Type { get; set; }

    [BsonElement("fullPath")] public string? FullPath { get; set; }

    [BsonElement("pathIds")] public string? PathIds { get; set; }

    [BsonElement("countryId")] public long? CountryId { get; set; }

    [BsonElement("description")] public string? Description { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("childrenIds")] public List<long> ChildrenIds { get; set; } = new();

    [BsonElement("assetIds")] public List<long> AssetIds { get; set; } = new();

    public LocationMongo()
    {
    }

    public LocationMongo(long domainId) : base(domainId)
    {
    }
}