using FAM.Application.Authorization.Roles.Shared;

using MediatR;

namespace FAM.Application.Authorization.Roles.Queries.GetRoleById;

/// <summary>
/// Query to get role by ID with permissions
/// </summary>
public sealed record GetRoleByIdQuery(long Id) : IRequest<RoleWithPermissionsDto?>;
