using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using MediatR;

namespace FAM.Application.Users.Queries;

/// <summary>
/// Query to get node roles for a specific user
/// Endpoint: GET /api/users/{userId}/roles
/// Supports including nested Role and Node details via ?include=role,node
/// </summary>
public sealed record GetUserRolesQuery : IRequest<PageResult<UserNodeRoleDto>>
{
    public long UserId { get; init; }
    public string? Filter { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Include { get; init; }  // Can include "role", "node", or "role,node"
}
