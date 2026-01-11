using FAM.Domain.Authorization;

namespace FAM.Application.Authorization.Permissions.Shared;

/// <summary>
/// Shared DTO for Permission - used across Commands and Queries (both list and detail)
/// </summary>
public sealed record PermissionDto(
    long Id,
    string Resource,
    string Action,
    string? Description,
    string PermissionKey,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt
);

public static class PermissionExtensions
{
    /// <summary>
    /// Convert Domain Permission entity to PermissionDto with conditional includes
    /// </summary>
    public static PermissionDto? ToPermissionDto(this Permission? permission, HashSet<string>? includes = null)
    {
        if (permission == null)
        {
            return null;
        }

        includes ??= new HashSet<string>();

        return new PermissionDto(
            permission.Id,
            permission.Resource,
            permission.Action,
            permission.Description,
            permission.GetPermissionKey(),
            permission.CreatedAt,
            permission.UpdatedAt,
            permission.DeletedAt
        );
    }
}
