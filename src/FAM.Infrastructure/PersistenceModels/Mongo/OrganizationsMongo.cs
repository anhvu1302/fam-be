using FAM.Infrastructure.PersistenceModels.Mongo.Base;

using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for OrgNode
/// </summary>
public class OrgNodeMongo : FullAuditedEntityMongo
{
    [BsonElement("type")] public int Type { get; set; } // OrgNodeType as int

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("parentId")] public long? ParentId { get; set; }

    [BsonElement("children")] public List<long> ChildrenIds { get; set; } = new();

    public OrgNodeMongo() : base()
    {
    }

    public OrgNodeMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// MongoDB document model for CompanyDetails
/// </summary>
public class CompanyDetailsMongo : BasicAuditedEntityMongo
{
    [BsonElement("nodeId")] public long NodeId { get; set; }

    [BsonElement("taxCode")] public string? TaxCode { get; set; }

    [BsonElement("domain")] public string? Domain { get; set; }

    [BsonElement("address")] public string? Address { get; set; }

    [BsonElement("establishedOn")] public DateTime? EstablishedOn { get; set; }

    public CompanyDetailsMongo() : base()
    {
    }

    public CompanyDetailsMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// MongoDB document model for DepartmentDetails
/// </summary>
public class DepartmentDetailsMongo : BasicAuditedEntityMongo
{
    [BsonElement("nodeId")] public long NodeId { get; set; }

    [BsonElement("costCenter")] public string? CostCenter { get; set; }

    [BsonElement("headcount")] public int? Headcount { get; set; }

    [BsonElement("budgetYear")] public decimal? BudgetYear { get; set; }

    public DepartmentDetailsMongo() : base()
    {
    }

    public DepartmentDetailsMongo(long domainId) : base(domainId)
    {
    }
}
