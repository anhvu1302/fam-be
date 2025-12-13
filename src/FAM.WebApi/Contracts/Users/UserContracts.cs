namespace FAM.WebApi.Contracts.Users;

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
