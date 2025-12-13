using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.AssignUsersToRole;

/// <summary>
/// Command to assign multiple users to a role (Batch Add)
/// </summary>
public sealed record AssignUsersToRoleCommand : IRequest<int>
{
    public long RoleId { get; init; }
    public long NodeId { get; init; }
    public long[] UserIds { get; init; } = Array.Empty<long>();
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public long? AssignedById { get; init; }
}
