using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.RevokePermissionsFromRole;

/// <summary>
/// Command to revoke permissions from a role
/// </summary>
public sealed record RevokePermissionsFromRoleCommand : IRequest<bool>
{
    public long RoleId { get; init; }
    public IReadOnlyList<long> PermissionIds { get; init; } = Array.Empty<long>();
}