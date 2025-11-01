using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for Permission
/// </summary>
public class PermissionMongo : BaseEntityMongo
{
    [BsonElement("resource")]
    public string Resource { get; set; } = string.Empty;

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    public PermissionMongo() : base() { }

    public PermissionMongo(long domainId) : base(domainId) { }
}

/// <summary>
/// MongoDB document model for Role
/// </summary>
public class RoleMongo : BaseEntityMongo
{
    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("rank")]
    public int Rank { get; set; }

    public RoleMongo() : base() { }

    public RoleMongo(long domainId) : base(domainId) { }
}

/// <summary>
/// MongoDB document model for Resource
/// </summary>
public class ResourceMongo : BaseEntityMongo
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("nodeId")]
    public long NodeId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    public ResourceMongo() : base() { }

    public ResourceMongo(long domainId) : base(domainId) { }
}

/// <summary>
/// MongoDB document model for RolePermission
/// </summary>
public class RolePermissionMongo : BaseEntityMongo
{
    [BsonElement("roleId")]
    public long RoleId { get; set; }

    [BsonElement("permissionId")]
    public long PermissionId { get; set; }

    public RolePermissionMongo() : base() { }

    public RolePermissionMongo(long domainId) : base(domainId) { }
}

/// <summary>
/// MongoDB document model for UserNodeRole
/// </summary>
public class UserNodeRoleMongo : BaseEntityMongo
{
    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("nodeId")]
    public long NodeId { get; set; }

    [BsonElement("roleId")]
    public long RoleId { get; set; }

    public UserNodeRoleMongo() : base() { }

    public UserNodeRoleMongo(long domainId) : base(domainId) { }
}