using FAM.Domain.Users;

namespace FAM.Application.Users.Shared;

/// <summary>
/// Shared DTO for User - used across Commands and Queries
/// </summary>
public sealed record UserDto(
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
    DateTime? DeletedAt,
    // Related entities (via includes)
    List<UserDeviceRef>? Devices,
    List<UserNodeRoleRef>? NodeRoles
);

/// <summary>
/// User device reference for user details
/// </summary>
public sealed record UserDeviceRef(
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
/// User node role reference for user details
/// Junction entity - identified by composite key (UserId, NodeId, RoleId)
/// </summary>
public sealed record UserNodeRoleRef(
    long UserId,
    long NodeId,
    string? NodeName,
    long RoleId,
    string? RoleName,
    DateTime? StartAt,
    DateTime? EndAt
);

public static class UserExtensions
{
    /// <summary>
    /// Convert Domain User entity to UserDto
    /// </summary>
    public static UserDto? ToUserDto(this User? user, HashSet<string>? includes = null)
    {
        if (user == null) return null;

        includes ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Conditionally include devices
        List<UserDeviceRef>? devices = null;
        if (includes.Contains("devices") || includes.Contains("userdevices"))
            devices = user.UserDevices?.Select(ud => new UserDeviceRef(
                ud.Id,
                ud.DeviceId,
                ud.DeviceName,
                ud.DeviceType.ToString(),
                ud.Browser,
                ud.OperatingSystem,
                ud.Location,
                ud.LastLoginAt,
                ud.LastActivityAt,
                ud.IsActive,
                ud.IsTrusted
            )).ToList() ?? new List<UserDeviceRef>();

        // Conditionally include node roles
        List<UserNodeRoleRef>? nodeRoles = null;
        if (includes.Contains("noderoles") || includes.Contains("usernoderoles"))
            nodeRoles = user.UserNodeRoles?.Select(unr => new UserNodeRoleRef(
                unr.UserId,
                unr.NodeId,
                unr.Node?.Name,
                unr.RoleId,
                unr.Role?.Name,
                unr.StartAt,
                unr.EndAt
            )).ToList() ?? new List<UserNodeRoleRef>();

        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.FirstName,
            user.LastName,
            user.Avatar,
            user.PhoneNumber,
            user.DateOfBirth,
            user.Bio,
            user.TwoFactorEnabled,
            user.IsActive,
            user.IsEmailVerified,
            user.IsPhoneVerified,
            user.EmailVerifiedAt,
            user.PhoneVerifiedAt,
            user.LastLoginAt,
            user.LastLoginIp,
            user.PreferredLanguage,
            user.TimeZone,
            user.ReceiveNotifications,
            user.ReceiveMarketingEmails,
            user.CreatedAt,
            user.UpdatedAt,
            user.DeletedAt,
            devices,
            nodeRoles
        );
    }

    /// <summary>
    /// Convert Domain User entity to minimal UserDto (for audit references)
    /// </summary>
    public static UserDto? ToUserDtoMinimal(this User? user)
    {
        if (user == null) return null;

        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.FirstName,
            user.LastName,
            user.Avatar,
            null, // PhoneNumber
            null, // DateOfBirth
            null, // Bio
            false, // TwoFactorEnabled
            user.IsActive,
            false, // IsEmailVerified
            false, // IsPhoneVerified
            null, // EmailVerifiedAt
            null, // PhoneVerifiedAt
            null, // LastLoginAt
            null, // LastLoginIp
            null, // PreferredLanguage
            null, // TimeZone
            false, // ReceiveNotifications
            false, // ReceiveMarketingEmails
            user.CreatedAt,
            user.UpdatedAt,
            null, // DeletedAt
            null, // Devices
            null // NodeRoles
        );
    }
}
