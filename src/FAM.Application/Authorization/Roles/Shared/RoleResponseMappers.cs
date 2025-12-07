using FAM.Application.Querying;

namespace FAM.Application.Authorization.Roles.Shared;

/// <summary>
/// Extension methods for mapping Role DTOs to responses
/// </summary>
public static class RoleResponseMappers
{
    /// <summary>
    /// Convert RoleDto to RoleResponse
    /// </summary>
    public static RoleResponse ToRoleResponse(this RoleDto dto)
    {
        return new RoleResponse(
            dto.Id,
            dto.Code,
            dto.Name,
            dto.Description,
            dto.Rank,
            dto.IsSystemRole,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.DeletedAt
        );
    }

    /// <summary>
    /// Convert PageResult of RoleDto to standard paged response
    /// </summary>
    public static object ToPagedResponse(this PageResult<RoleDto> result)
    {
        return new
        {
            data = result.Items.Select(p => p.ToRoleResponse()),
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
