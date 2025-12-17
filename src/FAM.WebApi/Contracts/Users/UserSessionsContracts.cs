using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.Users;

/// <summary>
/// Request to update user theme preferences
/// </summary>
[SwaggerSchema(Required = new[] { "theme", "transparency", "borderRadius", "darkTheme", "pinNavbar", "compactMode" })]
public record UpdateUserThemeRequest(
    string Theme,
    string? PrimaryColor,
    decimal Transparency,
    int BorderRadius,
    bool DarkTheme,
    bool PinNavbar,
    bool CompactMode
);
