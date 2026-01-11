using FAM.Application.Authorization.Roles.Commands.ReplaceUserRoles;
using FAM.Application.Common;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using FAM.Application.Settings;
using FAM.Application.Users.Commands;
using FAM.Application.Users.Commands.CreateUser;
using FAM.Application.Users.Commands.DeleteUser;
using FAM.Application.Users.Commands.UpdateUser;
using FAM.Application.Users.Queries.GetUserById;
using FAM.Application.Users.Queries.GetUsers;
using FAM.Application.Users.Shared;
using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Authorization;
using FAM.WebApi.Contracts.Common;
using FAM.WebApi.Contracts.Users;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Users API Controller - Web API layer validates shape/format before delegating to Application
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _pagination;

    public UsersController(IMediator mediator, PaginationSettings pagination)
    {
        _mediator = mediator;
        _pagination = pagination;
    }

    /// <summary>
    /// Get paginated list of users with advanced filtering and sorting
    /// </summary>
    /// <remarks>
    /// Returns paginated users with support for filtering, sorting, field selection, and eager loading of related entities.
    /// 
    /// Example request: GET /api/users?page=1&amp;pageSize=10&amp;sort=-createdAt&amp;filter=isActive:true&amp;fields=id,username,email&amp;include=devices,nodeRoles
    /// </remarks>
    /// <param name="parameters">Query parameters for pagination, filtering, sorting, field selection and includes</param>
    /// <response code="200">Success - Returns {success: true, result: UsersPagedResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<UsersPagedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiSuccessResponse<PageResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQueryParameters parameters)
    {
        GetUsersQuery query = new(parameters.ToQueryRequest());
        PageResult<UserDto> result = await _mediator.Send(query);

        // Apply field selection if requested
        if (parameters.GetFieldsArray() != null && parameters.GetFieldsArray()!.Length > 0)
        {
            object selectedResult = result.SelectFieldsToResponse(parameters.GetFieldsArray()!);
            return OkResponse(selectedResult);
        }

        return OkResponse(result.ToUsersPagedResponse());
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed user information by user ID with optional eager loading of related entities.
    /// 
    /// Example: GET /api/users/123?include=devices,nodeRoles
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="include">Include related entities (comma-separated, e.g., "userDevices,userNodeRoles" or "userNodeRoles.role")</param>
    /// <response code="200">Success - Returns {success: true, result: UserResponse}</response>
    /// <response code="404">User not found - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(long id, [FromQuery] string? include = null)
    {
        GetUserByIdQuery query = new(id, include);
        UserDto? result = await _mediator.Send(query);

        if (result == null)
        {
            throw new NotFoundException(ErrorCodes.GEN_NOT_FOUND, "User", id);
        }

        return OkResponse(result.ToUserResponse());
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <remarks>
    /// Creates a new user account with username, email, password and optional profile information.
    /// </remarks>
    /// <response code="201">User created successfully - Returns {success: true, result: UserResponse}</response>
    /// <response code="400">Bad request - Validation failed - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="409">Conflict - Username or email already exists - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Web API validation: ModelState checks DataAnnotations

        // Map Web API request → Application Command using extension method
        CreateUserCommand command = request.ToCommand();
        Application.Common.Result<CreateUserResult> result = await _mediator.Send(command);

        return result.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetUserById), new { id = success.User.Id },
                success.User.ToUserResponse()),
            (error, errorType) => MapErrorToResponse(error, errorType)
        );
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <remarks>
    /// Updates user profile information. Omitted fields will not be modified.
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <response code="200">User updated successfully - Returns {success: true, result: UserResponse}</response>
    /// <response code="400">Bad request - Validation failed - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">User not found - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="409">Conflict - Username or email already exists - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequest request)
    {
        // Web API validation: ModelState checks DataAnnotations

        // Map Web API request → Application Command using extension method
        UpdateUserCommand command = request.ToCommand(id);
        Application.Common.Result<UpdateUserResult> result = await _mediator.Send(command);

        return result.Match<IActionResult>(
            success => Ok(success.User.ToUserResponse()),
            (error, errorType) => MapErrorToResponse(error, errorType)
        );
    }

    /// <summary>
    /// Maps ErrorType to appropriate HTTP response per RFC 7231 / RFC 4918
    /// </summary>
    private IActionResult MapErrorToResponse(string error, ErrorType errorType)
    {
        var problemDetails = new
        {
            Error = error,
            Type = $"https://httpstatuses.com/{(int)GetStatusCode(errorType)}"
        };

        return errorType switch
        {
            ErrorType.NotFound => NotFound(problemDetails),
            ErrorType.Conflict => Conflict(problemDetails),
            ErrorType.Unauthorized => Unauthorized(problemDetails),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problemDetails),
            ErrorType.UnprocessableEntity => UnprocessableEntity(problemDetails),
            _ => BadRequest(problemDetails)
        };
    }

    private static int GetStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.UnprocessableEntity => 422,
            _ => 400
        };
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(long id)
    {
        DeleteUserCommand command = new(id);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Update user avatar using upload session pattern
    /// Step 2 after client has uploaded file to presigned URL
    /// </summary>
    /// <remarks>
    /// Updates user avatar after file has been uploaded to presigned URL. Completes the two-step upload process.
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="request">UpdateAvatarRequest with uploadId from InitUploadSession</param>
    /// <response code="200">Success - Returns {success: true, result: UpdateAvatarResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">User not found - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPut("{id:long}/avatar")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateAvatarResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvatar(long id, [FromBody] UpdateAvatarRequest request)
    {
        UpdateAvatarCommand command = new()
        {
            UserId = id,
            UploadId = request.UploadId
        };

        UpdateAvatarResponse result = await _mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Upload user avatar directly (single-step upload with transaction safety)
    /// Uploads file to storage, then updates user avatar atomically
    /// If DB update fails, uploaded file is automatically cleaned up
    /// </summary>
    /// <remarks>
    /// Single-step avatar upload. File is uploaded directly to storage and user record is updated in one atomic transaction.
    /// Maximum file size: 10 MB
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="file">Image file (jpg, png, gif, webp)</param>
    /// <response code="200">Success - Returns {success: true, result: UploadAvatarDirectResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">User not found - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPost("{id:long}/avatar/upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB max for avatars
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UploadAvatarDirectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAvatarDirect(long id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException("File is required", ErrorCodes.GEN_INVALID_OPERATION);
        }

        UploadAvatarDirectCommand command = new()
        {
            UserId = id,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length
        };

        UploadAvatarDirectResponse result = await _mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Replace all roles for a user at a node (Batch Replace)
    /// Used when editing a user and selecting roles
    /// </summary>
    /// <remarks>
    /// Replaces all roles for a user at a specific node. This is a batch operation that removes all existing roles and assigns new ones atomically.
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="request">ReplaceUserRolesRequest with nodeId and roleIds</param>
    /// <response code="200">Success - Returns {success: true, result: {message: "Successfully updated roles for user {id}"}}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">User not found - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPut("{id:long}/roles")]
    [ProducesResponseType(typeof(ApiSuccessResponse<dynamic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReplaceUserRoles(long id, [FromBody] ReplaceUserRolesRequest request)
    {
        long currentUserId = GetCurrentUserId();

        ReplaceUserRolesCommand command = new()
        {
            UserId = id,
            NodeId = request.NodeId,
            RoleIds = request.RoleIds,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            AssignedById = currentUserId
        };

        await _mediator.Send(command);

        return OkResponse(new { message = $"Successfully updated roles for user {id}" });
    }
}
