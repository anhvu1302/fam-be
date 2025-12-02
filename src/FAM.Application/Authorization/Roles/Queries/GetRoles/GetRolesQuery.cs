using FAM.Application.Authorization.Roles.Shared;
using FAM.Application.Querying;
using MediatR;

namespace FAM.Application.Authorization.Roles.Queries.GetRoles;

/// <summary>
/// Query to get paginated list of roles with filtering and sorting
/// </summary>
public sealed record GetRolesQuery(QueryRequest QueryRequest)
    : IRequest<PageResult<RoleDto>>;