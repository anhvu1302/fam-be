namespace FAM.Application.Authorization.Permissions.Shared;

/// <summary>
/// Response for permission data
/// </summary>
public sealed record PermissionResponse(
    long Id,
    string Resource,
    string Action,
    string? Description,
    string PermissionKey,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt
);