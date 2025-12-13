using FAM.Application.Authorization.Permissions.Shared;
using FAM.Domain.Authorization;

namespace FAM.Application.Authorization.Roles.Shared;

/// <summary>
/// Role DTO for API responses
/// </summary>
public sealed record RoleDto
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Rank { get; init; }
    public bool IsSystemRole { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}

/// <summary>
/// Role with permissions DTO
/// </summary>
public sealed record RoleWithPermissionsDto
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Rank { get; init; }
    public bool IsSystemRole { get; init; }
    public IReadOnlyList<PermissionDto> Permissions { get; init; } = Array.Empty<PermissionDto>();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// User role assignment DTO
/// </summary>
public sealed record UserRoleAssignmentDto
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public long NodeId { get; init; }
    public string NodeName { get; init; } = string.Empty;
    public long RoleId { get; init; }
    public string RoleCode { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public static class RoleExtensions
{
    /// <summary>
    /// Convert Domain Role entity to RoleDto with conditional includes
    /// </summary>
    public static RoleDto? ToRoleDto(this Role? role, HashSet<string>? includes = null)
    {
        if (role == null) return null;

        includes ??= new HashSet<string>();

        return new RoleDto
        {
            Id = role.Id,
            Code = role.Code.Value,
            Name = role.Name,
            Description = role.Description,
            Rank = role.Rank,
            IsSystemRole = role.IsSystemRole,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            DeletedAt = role.DeletedAt
        };
    }
}
