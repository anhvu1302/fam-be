using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Command to delete a role
/// </summary>
public sealed record DeleteRoleCommand(long Id) : IRequest<bool>;
