using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.AssignRoleToUser;

/// <summary>
/// Command to assign a role to a user at an organization node
/// </summary>
public sealed record AssignRoleToUserCommand : IRequest<bool>
{
    public long UserId { get; init; }
    public long NodeId { get; init; }
    public long RoleId { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public long? AssignedById { get; init; }
}
