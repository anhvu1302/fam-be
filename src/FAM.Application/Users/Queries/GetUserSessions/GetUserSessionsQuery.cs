using FAM.Application.Users.Shared;

using MediatR;

namespace FAM.Application.Users.Queries.GetUserSessions;

/// <summary>
/// Query to get all active login sessions for the current user
/// </summary>
public record GetUserSessionsQuery(long UserId) : IRequest<IReadOnlyList<UserSessionDto>>;
