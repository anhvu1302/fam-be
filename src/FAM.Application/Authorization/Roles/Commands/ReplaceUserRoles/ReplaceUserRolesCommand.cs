using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.ReplaceUserRoles;

/// <summary>
/// Command to replace all roles for a user at a node (Batch Replace)
/// </summary>
public sealed record ReplaceUserRolesCommand : IRequest
{
    public long UserId { get; init; }
    public long NodeId { get; init; }
    public long[] RoleIds { get; init; } = Array.Empty<long>();
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public long? AssignedById { get; init; }
}
