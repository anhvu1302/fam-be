namespace FAM.WebApi.Contracts.Users;

/// <summary>
/// Request to update user theme preferences
/// </summary>
public record UpdateUserThemeRequest(
    string Theme,
    string? PrimaryColor,
    decimal Transparency,
    int BorderRadius,
    bool DarkTheme,
    bool PinNavbar,
    bool CompactMode
);