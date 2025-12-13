using MediatR;

namespace FAM.Application.Users.Commands.UpdateUserTheme;

/// <summary>
/// Command to update or create user theme preferences
/// </summary>
public record UpdateUserThemeCommand(
    long UserId,
    string Theme,
    string? PrimaryColor,
    decimal Transparency,
    int BorderRadius,
    bool DarkTheme,
    bool PinNavbar,
    bool CompactMode
) : IRequest<UpdateUserThemeResponse>;
