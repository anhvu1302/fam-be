namespace FAM.Application.Users.Commands.UpdateUserTheme;

public record UpdateUserThemeResponse(
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
