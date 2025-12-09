using FAM.Domain.Common.Base;

namespace FAM.Domain.Authorization.Events;

/// <summary>
/// Domain event raised when a new role is created
/// </summary>
public sealed record RoleCreated(long RoleId, string Code, string Name, int Rank) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a role is updated
/// </summary>
public sealed record RoleUpdated(long RoleId, string Name, int Rank) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a role is deleted
/// </summary>
public sealed record RoleDeleted(long RoleId, string Code) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when permissions are assigned to a role
/// </summary>
public sealed record PermissionsAssignedToRole(long RoleId, IReadOnlyList<long> PermissionIds) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when permissions are revoked from a role
/// </summary>
public sealed record PermissionsRevokedFromRole(long RoleId, IReadOnlyList<long> PermissionIds) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a role is assigned to a user
/// </summary>
public sealed record RoleAssignedToUser(long UserId, long NodeId, long RoleId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a role is revoked from a user
/// </summary>
public sealed record RoleRevokedFromUser(long UserId, long NodeId, long RoleId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}