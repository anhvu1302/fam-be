using FAM.Application.Querying;
using FAM.Application.Settings;
using FAM.Application.Users.Commands.CreateUser;
using FAM.Application.Users.Commands.UpdateUser;
using FAM.Application.Users.Queries.GetUserById;
using FAM.Application.Users.Queries.GetUsers;
using FAM.WebApi.Contracts.Common;
using FAM.WebApi.Contracts.Users;

namespace FAM.WebApi.Mappers;

/// <summary>
/// Extension methods for mapping User requests to commands/queries
/// </summary>
public static class UserMappers
{
    /// <summary>
    /// Convert PaginationQueryParameters to GetUsersQuery
    /// </summary>
    public static GetUsersQuery ToGetUsersQuery(this PaginationQueryParameters parameters, PaginationSettings paginationSettings)
    {
        var queryRequest = new QueryRequest
        {
            Filter = parameters.Filter,
            Sort = parameters.Sort,
            Page = parameters.Page > 0 ? parameters.Page : 1,
            PageSize = parameters.PageSize > 0 ? parameters.PageSize : PaginationSettings.DefaultPageSize,
            Fields = parameters.GetFieldsArray(),
            Include = parameters.Include
        };

        return new GetUsersQuery(queryRequest);
    }

    /// <summary>
    /// Create GetUserByIdQuery
    /// </summary>
    public static GetUserByIdQuery ToGetUserByIdQuery(this long id, string? include = null)
    {
        return new GetUserByIdQuery(id, include);
    }

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
