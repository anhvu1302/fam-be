using FAM.Application.Querying;

namespace FAM.Application.Authorization.Permissions.Shared;

/// <summary>
/// Extension methods for mapping Permission DTOs to responses
/// </summary>
public static class PermissionResponseMappers
{
    /// <summary>
    /// Convert PermissionDto to PermissionResponse
    /// </summary>
    public static PermissionResponse ToPermissionResponse(this PermissionDto dto)
    {
        return new PermissionResponse(
            dto.Id,
            dto.Resource,
            dto.Action,
            dto.Description,
            dto.PermissionKey,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.DeletedAt
        );
    }

    /// <summary>
    /// Convert PageResult of PermissionDto to standard paged response
    /// </summary>
    public static object ToPagedResponse(this PageResult<PermissionDto> result)
    {
        return new
        {
            data = result.Items.Select(p => p.ToPermissionResponse()),
            pagination = new
            {
                page = result.Page,
                pageSize = result.PageSize,
                total = result.Total,
                totalPages = result.TotalPages,
                hasPrevPage = result.HasPrevPage,
                hasNextPage = result.HasNextPage
            }
        };
    }
}
