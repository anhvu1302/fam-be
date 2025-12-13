using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.RemoveUsersFromRole;

/// <summary>
/// Command to remove multiple users from a role (Batch Remove)
/// </summary>
public sealed record RemoveUsersFromRoleCommand : IRequest<int>
{
    public long RoleId { get; init; }
    public long NodeId { get; init; }
    public long[] UserIds { get; init; } = Array.Empty<long>();
}
