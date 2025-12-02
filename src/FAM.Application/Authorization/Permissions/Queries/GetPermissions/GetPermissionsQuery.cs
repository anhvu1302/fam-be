using FAM.Application.Authorization.Permissions.Shared;
using FAM.Application.Querying;
using MediatR;

namespace FAM.Application.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Query to get paginated list of permissions with filtering and sorting
/// </summary>
public sealed record GetPermissionsQuery(QueryRequest QueryRequest)
    : IRequest<PageResult<PermissionDto>>;