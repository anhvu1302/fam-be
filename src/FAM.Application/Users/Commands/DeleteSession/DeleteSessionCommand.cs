using MediatR;

namespace FAM.Application.Users.Commands.DeleteSession;

/// <summary>
/// Command to delete (remove) a specific login session and blacklist its token
/// </summary>
public record DeleteSessionCommand(
    long UserId,
    Guid SessionId,
    string? AccessToken = null,
    DateTime? AccessTokenExpiration = null
) : IRequest<Unit>;