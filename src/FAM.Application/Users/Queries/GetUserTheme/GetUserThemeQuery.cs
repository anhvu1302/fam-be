using MediatR;

namespace FAM.Application.Users.Queries.GetUserTheme;

/// <summary>
/// Query to get user's theme preferences
/// </summary>
public record GetUserThemeQuery(long UserId) : IRequest<GetUserThemeResponse?>;
