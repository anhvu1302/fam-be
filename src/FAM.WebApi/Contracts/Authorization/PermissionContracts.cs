namespace FAM.WebApi.Contracts.Authorization;

/// <summary>
/// Permission response DTO
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