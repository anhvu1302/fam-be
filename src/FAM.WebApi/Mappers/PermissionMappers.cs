using FAM.Application.Authorization.Permissions.Queries.GetPermissions;
using FAM.Application.Authorization.Permissions.Shared;
using FAM.Application.Querying;
using FAM.WebApi.Contracts.Authorization;

namespace FAM.WebApi.Mappers;

public static class PermissionMappers
{
    public static GetPermissionsQuery ToQuery(this QueryRequest request)
    {
        return new GetPermissionsQuery(request);
    }

    // public static GetPermissionByIdQuery ToQuery(long id, string? include = null)
    // {
    //     return new GetPermissionByIdQuery(id, include);
    // }

    // DTO -> Response (single mapper for all scenarios)
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

    // PageResult -> Response with pagination
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

    // Helper method to generate code from name
    private static string GenerateCodeFromName(string name)
    {
        return name.ToUpperInvariant().Replace(" ", "_");
    }
}