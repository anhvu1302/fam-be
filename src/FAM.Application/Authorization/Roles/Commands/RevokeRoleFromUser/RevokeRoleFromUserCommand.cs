using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.RevokeRoleFromUser;

/// <summary>
/// Command to revoke a role from a user
/// </summary>
public sealed record RevokeRoleFromUserCommand(
    long UserId,
    long NodeId,
    long RoleId
) : IRequest<bool>;