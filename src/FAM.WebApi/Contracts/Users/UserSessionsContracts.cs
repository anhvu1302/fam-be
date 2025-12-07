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

/// <summary>
/// Response containing user theme preferences
/// </summary>
public record UserThemeResponse(
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

/// <summary>
/// Response containing list of user's login sessions
/// </summary>
public record UserSessionsResponse(List<UserSessionResponse> Sessions);

/// <summary>
/// Individual session information
/// </summary>
public record UserSessionResponse(
    Guid Id,
    string DeviceId,
    string DeviceName,
    string DeviceType,
    string? IpAddress,
    string? Location,
    string? Browser,
    string? OperatingSystem,
    DateTime LastLoginAt,
    DateTime? LastActivityAt,
    bool IsActive,
    bool IsTrusted,
    bool IsCurrentDevice
);
