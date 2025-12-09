using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Users.Queries.GetUserTheme;

public class GetUserThemeQueryHandler : IRequestHandler<GetUserThemeQuery, GetUserThemeResponse?>
{
    private readonly IUserThemeRepository _userThemeRepository;

    public GetUserThemeQueryHandler(IUserThemeRepository userThemeRepository)
    {
        _userThemeRepository = userThemeRepository;
    }

    public async Task<GetUserThemeResponse?> Handle(GetUserThemeQuery request, CancellationToken cancellationToken)
    {
        var theme = await _userThemeRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (theme == null)
            return null;

        return new GetUserThemeResponse(
            theme.Id,
            theme.UserId,
            theme.Theme,
            theme.PrimaryColor,
            theme.Transparency,
            theme.BorderRadius,
            theme.DarkTheme,
            theme.PinNavbar,
            theme.CompactMode
        );
    }
}