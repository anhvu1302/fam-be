namespace FAM.Application.Users.Queries.GetUserTheme;

public record GetUserThemeResponse(
    long Id,
    long UserId,
    string Theme,
    string? PrimaryColor,
    decimal Transparency,
    int BorderRadius,
    bool DarkTheme,
    bool PinNavbar,
    bool CompactMode
);
