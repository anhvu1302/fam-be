namespace FAM.WebApi.Contracts.Users;

#region Request Contracts

/// <summary>
/// Request to create a new user - Validated by CreateUserRequestValidator
/// </summary>
public sealed record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null
);

/// <summary>
/// Request to update an existing user - Validated by UpdateUserRequestValidator
/// </summary>
public sealed record UpdateUserRequest(
    string? Username = null,
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    DateTime? DateOfBirth = null,
    string? Bio = null,
    string? PreferredLanguage = null,
    string? TimeZone = null,
    bool? ReceiveNotifications = null,
    bool? ReceiveMarketingEmails = null
);

/// <summary>
/// Request to update user avatar - Validated by UpdateAvatarRequestValidator
/// </summary>
public sealed record UpdateAvatarRequest(
    string UploadId
);

#endregion

#region Response Contracts

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
    long Id,
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

#endregion