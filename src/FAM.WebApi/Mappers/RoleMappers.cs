using FAM.Application.Authorization.Roles.Queries.GetRoles;
using FAM.Application.Authorization.Roles.Shared;
using FAM.Application.Querying;
using FAM.WebApi.Contracts.Authorization;

namespace FAM.WebApi.Mappers;

/// <summary>
/// Extension methods for mapping Role DTOs to API responses
/// </summary>
public static class RoleMappers
{
    public static GetRolesQuery ToQuery(this QueryRequest request)
    {
        return new GetRolesQuery(request);
    }

    // public static GetPermissionByIdQuery ToQuery(long id, string? include = null)
    // {
    //     return new GetPermissionByIdQuery(id, include);
    // }

    // DTO -> Response (single mapper for all scenarios)
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