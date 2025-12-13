using FAM.Infrastructure.PersistenceModels.Mongo.Base;

using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for Permission
/// </summary>
public class PermissionMongo : FullAuditedEntityMongo
{
    [BsonElement("resource")] public string Resource { get; set; } = string.Empty;

    [BsonElement("action")] public string Action { get; set; } = string.Empty;

    [BsonElement("description")] public string? Description { get; set; }

    public PermissionMongo() : base()
    {
    }

    public PermissionMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// MongoDB document model for Role
/// </summary>
public class RoleMongo : FullAuditedEntityMongo
{
    [BsonElement("code")] public string Code { get; set; } = string.Empty;

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("description")] public string? Description { get; set; }

    [BsonElement("rank")] public int Rank { get; set; }

    public RoleMongo() : base()
    {
    }

    public RoleMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// MongoDB document model for Resource
/// </summary>
public class ResourceMongo : BasicAuditedEntityMongo
{
    [BsonElement("type")] public string Type { get; set; } = string.Empty;

    [BsonElement("nodeId")] public long NodeId { get; set; }

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    public ResourceMongo() : base()
    {
    }

    public ResourceMongo(long domainId) : base(domainId)
    {
    }
}

/// <summary>
/// MongoDB document model for RolePermission
/// </summary>
public class RolePermissionMongo : JunctionEntityMongo
{
    [BsonElement("roleId")] public long RoleId { get; set; }

    [BsonElement("permissionId")] public long PermissionId { get; set; }

    [BsonElement("grantedById")] public long? GrantedById { get; set; }

    public RolePermissionMongo() : base()
    {
    }
}

/// <summary>
/// MongoDB document model for UserNodeRole
/// </summary>
public class UserNodeRoleMongo : JunctionEntityMongo
{
    [BsonElement("userId")] public long UserId { get; set; }

    [BsonElement("nodeId")] public long NodeId { get; set; }

    [BsonElement("roleId")] public long RoleId { get; set; }

    [BsonElement("startAt")] public DateTime? StartAt { get; set; }

    [BsonElement("endAt")] public DateTime? EndAt { get; set; }

    [BsonElement("assignedById")] public long? AssignedById { get; set; }

    public UserNodeRoleMongo() : base()
    {
    }
}
