using FAM.Application.Querying;

namespace FAM.Application.Users.Shared;

/// <summary>
/// Extension methods for mapping between User DTOs and Response types
/// </summary>
public static class UserResponseMappers
{
    /// <summary>
    /// Convert UserDto to UserResponse
    /// </summary>
    public static UserResponse ToUserResponse(this UserDto dto)
    {
        return new UserResponse(
            dto.Id,
            dto.Username,
            dto.Email,
            dto.FullName,
            dto.FirstName,
            dto.LastName,
            dto.Avatar,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.Bio,
            dto.TwoFactorEnabled,
            dto.IsActive,
            dto.IsEmailVerified,
            dto.IsPhoneVerified,
            dto.EmailVerifiedAt,
            dto.PhoneVerifiedAt,
            dto.LastLoginAt,
            dto.LastLoginIp,
            dto.PreferredLanguage,
            dto.TimeZone,
            dto.ReceiveNotifications,
            dto.ReceiveMarketingEmails,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.Devices?.Select(d => d.ToUserDeviceResponse()).ToList(),
            dto.NodeRoles?.Select(r => r.ToUserNodeRoleResponse()).ToList()
        );
    }

    /// <summary>
    /// Convert UserDeviceRef to UserDeviceResponse
    /// </summary>
    public static UserDeviceResponse ToUserDeviceResponse(this UserDeviceRef device)
    {
        return new UserDeviceResponse(
            device.Id,
            device.DeviceId,
            device.DeviceName,
            device.DeviceType,
            device.Browser,
            device.OperatingSystem,
            device.Location,
            device.LastLoginAt,
            device.LastActivityAt,
            device.IsActive,
            device.IsTrusted
        );
    }

    /// <summary>
    /// Convert UserNodeRoleRef to UserNodeRoleResponse
    /// </summary>
    public static UserNodeRoleResponse ToUserNodeRoleResponse(this UserNodeRoleRef nodeRole)
    {
        return new UserNodeRoleResponse(
            nodeRole.UserId,
            nodeRole.NodeId,
            nodeRole.NodeName,
            nodeRole.RoleId,
            nodeRole.RoleName,
            nodeRole.StartAt,
            nodeRole.EndAt
        );
    }

    /// <summary>
    /// Convert PageResult of UserDto to UsersPagedResponse
    /// </summary>
    public static UsersPagedResponse ToUsersPagedResponse(this PageResult<UserDto> pageResult)
    {
        return new UsersPagedResponse(
            pageResult.Items.Select(u => u.ToUserResponse()).ToList(),
            (int)pageResult.Total,
            pageResult.Page,
            pageResult.PageSize,
            pageResult.TotalPages
        );
    }
}
