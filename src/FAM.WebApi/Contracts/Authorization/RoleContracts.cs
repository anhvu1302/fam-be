namespace FAM.WebApi.Contracts.Authorization;

using Swashbuckle.AspNetCore.Annotations;

#region Request Contracts

/// <summary>
/// Request to create a new role
/// </summary>
[SwaggerSchema(Required = new[] { "code", "name", "rank" })]
public sealed record CreateRoleRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Rank { get; init; }
}

/// <summary>
/// Request to update role
/// </summary>
[SwaggerSchema(Required = new[] { "name", "rank" })]
public sealed record UpdateRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Rank { get; init; }
}

/// <summary>
/// Request to assign permissions to role
/// </summary>
[SwaggerSchema(Required = new[] { "permissionIds" })]
public sealed record AssignPermissionsRequest
{
    public long[] PermissionIds { get; init; } = Array.Empty<long>();
}

/// <summary>
/// Request to assign role to user
/// </summary>
[SwaggerSchema(Required = new[] { "userId", "nodeId", "roleId" })]
public sealed record AssignRoleToUserRequest
{
    public long UserId { get; init; }
    public long NodeId { get; init; }
    public long RoleId { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
}

/// <summary>
/// Request to assign multiple users to a role (Batch Add)
/// </summary>
[SwaggerSchema(Required = new[] { "nodeId", "userIds" })]
public sealed record AssignUsersToRoleRequest
{
    public long NodeId { get; init; }
    public long[] UserIds { get; init; } = Array.Empty<long>();
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
}

/// <summary>
/// Request to remove users from a role (Batch Remove)
/// </summary>
[SwaggerSchema(Required = new[] { "nodeId", "userIds" })]
public sealed record RemoveUsersFromRoleRequest
{
    public long NodeId { get; init; }
    public long[] UserIds { get; init; } = Array.Empty<long>();
}

/// <summary>
/// Request to replace all roles for a user (Batch Replace)
/// </summary>
[SwaggerSchema(Required = new[] { "nodeId", "roleIds" })]
public sealed record ReplaceUserRolesRequest
{
    public long NodeId { get; init; }
    public long[] RoleIds { get; init; } = Array.Empty<long>();
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
}

#endregion
