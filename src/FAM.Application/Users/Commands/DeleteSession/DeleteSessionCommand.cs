using MediatR;

namespace FAM.Application.Users.Commands.DeleteSession;

/// <summary>
/// Command to delete (deactivate) a specific login session
/// </summary>
public record DeleteSessionCommand(long UserId, Guid SessionId) : IRequest<Unit>;