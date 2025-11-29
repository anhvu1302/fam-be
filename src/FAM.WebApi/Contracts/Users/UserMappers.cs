using FAM.Application.Querying;
using FAM.Application.Users.Commands.CreateUser;
using FAM.Application.Users.Commands.UpdateUser;
using FAM.Application.Users.Queries.GetUserById;
using FAM.Application.Users.Queries.GetUsers;
using FAM.Application.Users.Shared;

namespace FAM.WebApi.Contracts.Users;

/// <summary>
/// Extension methods for mapping between WebApi contracts and Application layer
/// </summary>
public static class UserMappers
{
    #region Request to Command Mappers

    /// <summary>
    /// Convert CreateUserRequest to CreateUserCommand
    /// </summary>
    public static CreateUserCommand ToCommand(this CreateUserRequest request)
    {
        return new CreateUserCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );
    }

    /// <summary>
    /// Convert UpdateUserRequest to UpdateUserCommand
    /// </summary>
    public static UpdateUserCommand ToCommand(this UpdateUserRequest request, long id)
    {
        return new UpdateUserCommand(
            id,
            request.Username,
            request.Email,
            null, // Password should be updated separately with proper verification
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Bio,
            request.DateOfBirth,
            request.PreferredLanguage,
            request.TimeZone,
            request.ReceiveNotifications,
            request.ReceiveMarketingEmails
        );
    }

    #endregion

    #region Query Request Mappers

    /// <summary>
    /// Convert QueryRequest to GetUsersQuery
    /// </summary>
    public static GetUsersQuery ToGetUsersQuery(this QueryRequest queryRequest)
    {
        return new GetUsersQuery(queryRequest);
    }

    /// <summary>
    /// Create GetUserByIdQuery
    /// </summary>
    public static GetUserByIdQuery ToGetUserByIdQuery(this long id, string? include = null)
    {
        return new GetUserByIdQuery(id, include);
    }

    #endregion

    #region DTO to Response Mappers

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
            nodeRole.Id,
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

    #endregion
}