using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.AssignPermissionsToRole;

/// <summary>
/// Command to assign permissions to a role
/// </summary>
public sealed record AssignPermissionsToRoleCommand : IRequest<bool>
{
    public long RoleId { get; init; }
    public IReadOnlyList<long> PermissionIds { get; init; } = Array.Empty<long>();
    public long? AssignedById { get; init; }
}
