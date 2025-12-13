namespace FAM.Application.Users.Shared;

/// <summary>
/// Response for user data
/// </summary>
public sealed record UserResponse(
    long Id,
    string Username,
    string Email,
    string? FullName,
    string? FirstName,
    string? LastName,
    string? Avatar,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Bio,
    bool TwoFactorEnabled,
    bool IsActive,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    DateTime? EmailVerifiedAt,
    DateTime? PhoneVerifiedAt,
    DateTime? LastLoginAt,
    string? LastLoginIp,
    string? PreferredLanguage,
    string? TimeZone,
    bool ReceiveNotifications,
    bool ReceiveMarketingEmails,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<UserDeviceResponse>? Devices = null,
    List<UserNodeRoleResponse>? NodeRoles = null
);

/// <summary>
/// Response for user device reference
/// </summary>
public sealed record UserDeviceResponse(
    Guid Id,
    string DeviceId,
    string DeviceName,
    string DeviceType,
    string? Browser,
    string? OperatingSystem,
    string? Location,
    DateTime LastLoginAt,
    DateTime? LastActivityAt,
    bool IsActive,
    bool IsTrusted
);

/// <summary>
/// Response for user node role reference
/// </summary>
public sealed record UserNodeRoleResponse(
    long UserId,
    long NodeId,
    string? NodeName,
    long RoleId,
    string? RoleName,
    DateTime? StartAt,
    DateTime? EndAt
);

/// <summary>
/// Paginated response for users
/// </summary>
public sealed record UsersPagedResponse(
    List<UserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Response for theme preferences
/// </summary>
public sealed record UserThemeResponse(
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
/// <summary>
/// Individual session information
/// </summary>
public sealed record UserSessionResponse(
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
