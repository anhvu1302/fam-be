using MediatR;

namespace FAM.Application.Users.Commands.DeleteAllSessions;

/// <summary>
/// Command to delete all login sessions except optionally the current one
/// </summary>
public record DeleteAllSessionsCommand(long UserId, string? ExcludeDeviceId = null) : IRequest<Unit>;