using System.ComponentModel.DataAnnotations;

namespace FAM.WebApi.Contracts.Users;

#region Request Contracts

/// <summary>
/// Request to create a new user
/// </summary>
public sealed record CreateUserRequest(
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$",
        ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    string Username,
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    string Email,
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    string Password,
    [StringLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    string? FirstName = null,
    [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    string? LastName = null,
    [Phone(ErrorMessage = "Invalid phone number format")]
    string? PhoneNumber = null
);

/// <summary>
/// Request to update an existing user
/// </summary>
public sealed record UpdateUserRequest(
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$",
        ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    string? Username = null,
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    string? Email = null,
    [StringLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    string? FirstName = null,
    [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    string? LastName = null,
    [Phone(ErrorMessage = "Invalid phone number format")]
    string? PhoneNumber = null,
    DateTime? DateOfBirth = null,
    [StringLength(500, ErrorMessage = "Bio must not exceed 500 characters")]
    string? Bio = null,
    [StringLength(10, ErrorMessage = "Language code must not exceed 10 characters")]
    string? PreferredLanguage = null,
    [StringLength(50, ErrorMessage = "Timezone must not exceed 50 characters")]
    string? TimeZone = null,
    bool? ReceiveNotifications = null,
    bool? ReceiveMarketingEmails = null
);

/// <summary>
/// Request to update user avatar
/// </summary>
public sealed record UpdateAvatarRequest(
    [Required(ErrorMessage = "Upload ID is required")]
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