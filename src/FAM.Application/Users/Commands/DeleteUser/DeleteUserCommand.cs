using MediatR;

namespace FAM.Application.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete a user
/// </summary>
public sealed record DeleteUserCommand(long Id) : IRequest<bool>;
