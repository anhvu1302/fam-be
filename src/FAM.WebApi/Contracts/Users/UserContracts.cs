using FAM.Application.Users.Commands.CreateUser;
using FAM.Application.Users.Commands.UpdateUser;
using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.Users;

/// <summary>
/// Request to create a new user - Validated by CreateUserRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "username", "email", "password" })]
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
[SwaggerSchema(Required = new[] { "uploadId" })]
public sealed record UpdateAvatarRequest(
    string UploadId
);

#region Mapper Extension Methods

/// <summary>
/// Extension methods for mapping User requests to commands/queries
/// </summary>
public static class UserContractMappers
{
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
}

#endregion
