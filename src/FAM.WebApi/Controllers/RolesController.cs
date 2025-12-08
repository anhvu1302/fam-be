using FAM.Application.Authorization.Roles.Commands.AssignPermissionsToRole;
using FAM.Application.Authorization.Roles.Commands.AssignRoleToUser;
using FAM.Application.Authorization.Roles.Commands.AssignUsersToRole;
using FAM.Application.Authorization.Roles.Commands.CreateRole;
using FAM.Application.Authorization.Roles.Commands.DeleteRole;
using FAM.Application.Authorization.Roles.Commands.RemoveUsersFromRole;
using FAM.Application.Authorization.Roles.Commands.RevokePermissionsFromRole;
using FAM.Application.Authorization.Roles.Commands.RevokeRoleFromUser;
using FAM.Application.Authorization.Roles.Commands.UpdateRole;
using FAM.Application.Authorization.Roles.Queries.GetRoles;
using FAM.Application.Authorization.Roles.Queries.GetRoleById;
using FAM.Application.Authorization.Roles.Shared;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using FAM.WebApi.Contracts.Authorization;
using FAM.WebApi.Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Role management controller
/// Manages roles and role assignments
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles with pagination, filtering and sorting
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of roles. Supports filtering, sorting, field selection, and includes.
    /// 
    /// Query Parameters:
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 10)
    /// - sortBy: Field to sort by (e.g., "id", "code", "name", "rank")
    /// - sortOrder: Sort order - "asc" or "desc" (default: asc)
    /// - fields: Comma-separated fields to include in response
    /// - search: Search keyword for filtering roles
    /// 
    /// Example: GET /api/roles?page=1&amp;pageSize=10&amp;sortBy=name&amp;sortOrder=asc
    /// </remarks>
    /// <param name="parameters">Query parameters for pagination, filtering, sorting, field selection and includes</param>
    /// <response code="200">Success - Returns {success: true, result: {items: [Role], totalCount, pageNumber, pageSize, totalPages}}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid query parameters", code: "INVALID_PARAMETERS"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<PageResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRoles([FromQuery] PaginationQueryParameters parameters)
    {
        var query = new GetRolesQuery(parameters.ToQueryRequest());
        var result = await _mediator.Send(query);

        // Apply field selection if requested
        var fields = parameters.GetFieldsArray();
        if (fields != null && fields.Length > 0)
        {
            var selectedResult = result.SelectFieldsToResponse(fields);
            return OkResponse(selectedResult);
        }

        return OkResponse(result.ToPagedResponse());
    }

    /// <summary>
    /// Get role by ID with permissions
    /// </summary>
    /// <remarks>
    /// Returns a specific role by ID including all assigned permissions.
    /// 
    /// Example: GET /api/roles/1
    /// 
    /// Returns role details with full permission list.
    /// </remarks>
    /// <param name="id">Role ID</param>
    /// <response code="200">Success - Returns {success: true, result: RoleWithPermissionsDto}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Role with ID {id} not found", code: "ROLE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<RoleWithPermissionsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleWithPermissionsDto>> GetRoleById(long id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFoundResponse($"Role with ID {id} not found", "ROLE_NOT_FOUND");

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <remarks>
    /// Creates a new role with the specified properties.
    /// 
    /// Request body:
    /// {
    ///   "code": "admin",
    ///   "name": "Administrator",
    ///   "description": "Administrator role with full permissions",
    ///   "rank": 1
    /// }
    /// 
    /// System roles (IsSystemRole=true) can only be created by the system.
    /// Roles with lower rank have higher priority.
    /// 
    /// Example: POST /api/roles
    /// </remarks>
    /// <param name="request">CreateRoleRequest with role details</param>
    /// <response code="201">Created - Returns {success: true, result: long (roleId)}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid role data", code: "INVALID_ROLE_DATA"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="409">Conflict - Returns {success: false, errors: [{message: "Role with code {code} already exists", code: "ROLE_ALREADY_EXISTS"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<long>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Rank = request.Rank,
            IsSystemRole = false // Only system can create system roles
        };

        var roleId = await _mediator.Send(command);

        _logger.LogInformation("Role created: {RoleId} - {RoleCode}", roleId, request.Code);

        return CreatedAtAction(nameof(GetRoleById), new { id = roleId }, roleId);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    /// <remarks>
    /// Updates an existing role with new values.
    /// 
    /// Request body:
    /// {
    ///   "name": "Updated Role Name",
    ///   "description": "Updated description",
    ///   "rank": 2
    /// }
    /// 
    /// Only the fields provided in the request will be updated.
    /// System roles cannot be modified.
    /// 
    /// Example: PATCH /api/roles/1
    /// </remarks>
    /// <param name="id">Role ID to update</param>
    /// <param name="request">UpdateRoleRequest with role details to update</param>
    /// <response code="200">Success - Returns {success: true, message: "Role updated successfully"}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid role data", code: "INVALID_ROLE_DATA"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Role with ID {id} not found", code: "ROLE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPatch("{id:long}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateRole(long id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Rank = request.Rank
        };

        await _mediator.Send(command);

        _logger.LogInformation("Role updated: {RoleId}", id);

        return OkResponse("Role updated successfully");
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    /// <remarks>
    /// Deletes an existing role. Cannot delete roles that are assigned to users or system roles.
    /// 
    /// Example: DELETE /api/roles/1
    /// 
    /// Before deleting a role, all user-role assignments must be removed.
    /// </remarks>
    /// <param name="id">Role ID to delete</param>
    /// <response code="204">Success - No content returned</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Role with ID {id} not found", code: "ROLE_NOT_FOUND"}]}</response>
    /// <response code="409">Conflict - Returns {success: false, errors: [{message: "Cannot delete role with assigned users", code: "ROLE_IN_USE"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteRole(long id)
    {
        var command = new DeleteRoleCommand(id);
        await _mediator.Send(command);

        _logger.LogInformation("Role deleted: {RoleId}", id);

        return OkResponse("Role deleted successfully");
    }

    /// <summary>
    /// Assign permissions to a role (Batch Replace)
    /// Replaces all existing permissions with new ones
    /// </summary>
    [HttpPut("{id:long}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignPermissions(long id, [FromBody] AssignPermissionsRequest request)
    {
        var userId = GetCurrentUserId();

        var command = new AssignPermissionsToRoleCommand
        {
            RoleId = id,
            PermissionIds = request.PermissionIds,
            AssignedById = userId
        };

        await _mediator.Send(command);

        _logger.LogInformation("Permissions assigned to role: {RoleId}, Count: {Count}", id,
            request.PermissionIds.Length);

        return OkResponse($"Successfully assigned {request.PermissionIds.Length} permission(s) to role");
    }

    /// <summary>
    /// Revoke permissions from a role
    /// </summary>
    [HttpDelete("{id:long}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokePermissions(long id, [FromBody] AssignPermissionsRequest request)
    {
        var command = new RevokePermissionsFromRoleCommand
        {
            RoleId = id,
            PermissionIds = request.PermissionIds
        };

        await _mediator.Send(command);

        _logger.LogInformation("Permissions revoked from role: {RoleId}, Count: {Count}", id,
            request.PermissionIds.Length);

        return OkResponse();
    }

    /// <summary>
    /// Add multiple users to a role (Batch Add)
    /// Keeps existing assignments and adds new ones
    /// </summary>
    [HttpPost("{id:long}/users")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> AddUsersToRole(long id, [FromBody] AssignUsersToRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();

        var command = new AssignUsersToRoleCommand
        {
            RoleId = id,
            NodeId = request.NodeId,
            UserIds = request.UserIds,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            AssignedById = currentUserId
        };

        var addedCount = await _mediator.Send(command);

        _logger.LogInformation("Added {Count} users to role: {RoleId} at node {NodeId}",
            addedCount, id, request.NodeId);

        return OkResponse(new { addedCount, message = $"Successfully added {addedCount} user(s) to role" });
    }

    /// <summary>
    /// Remove multiple users from a role (Batch Remove)
    /// </summary>
    [HttpDelete("{id:long}/users")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> RemoveUsersFromRole(long id, [FromBody] RemoveUsersFromRoleRequest request)
    {
        var command = new RemoveUsersFromRoleCommand
        {
            RoleId = id,
            NodeId = request.NodeId,
            UserIds = request.UserIds
        };

        var removedCount = await _mediator.Send(command);

        _logger.LogInformation("Removed {Count} users from role: {RoleId} at node {NodeId}",
            removedCount, id, request.NodeId);

        return OkResponse(new { removedCount, message = $"Successfully removed {removedCount} user(s) from role" });
    }

    /// <summary>
    /// Assign role to user at organization node
    /// </summary>
    [HttpPost("assignments")]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> AssignRoleToUser([FromBody] AssignRoleToUserRequest request)
    {
        var currentUserId = GetCurrentUserId();

        var command = new AssignRoleToUserCommand
        {
            UserId = request.UserId,
            NodeId = request.NodeId,
            RoleId = request.RoleId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            AssignedById = currentUserId
        };

        var assignmentId = await _mediator.Send(command);

        _logger.LogInformation("Role assigned to user: UserId={UserId}, RoleId={RoleId}, NodeId={NodeId}",
            request.UserId, request.RoleId, request.NodeId);

        return CreatedAtAction(nameof(AssignRoleToUser), new { id = assignmentId }, assignmentId);
    }

    /// <summary>
    /// Revoke role from user
    /// </summary>
    [HttpDelete("assignments/{userId:long}/{nodeId:long}/{roleId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeRoleFromUser(long userId, long nodeId, long roleId)
    {
        var command = new RevokeRoleFromUserCommand(userId, nodeId, roleId);
        await _mediator.Send(command);

        _logger.LogInformation("Role assignment revoked: User={UserId}, Node={NodeId}, Role={RoleId}",
            userId, nodeId, roleId);

        return NoContent();
    }
}
