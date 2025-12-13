namespace FAM.Application.Authorization.Roles.Shared;

/// <summary>
/// Response for role data
/// </summary>
public sealed record RoleResponse(
    long Id,
    string Code,
    string Name,
    string? Description,
    int Rank,
    bool IsSystemRole,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt
);
