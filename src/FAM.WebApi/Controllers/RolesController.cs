using FAM.Application.Authorization.Roles.Commands.AssignPermissionsToRole;
using FAM.Application.Authorization.Roles.Commands.AssignRoleToUser;
using FAM.Application.Authorization.Roles.Commands.AssignUsersToRole;
using FAM.Application.Authorization.Roles.Commands.CreateRole;
using FAM.Application.Authorization.Roles.Commands.DeleteRole;
using FAM.Application.Authorization.Roles.Commands.RemoveUsersFromRole;
using FAM.Application.Authorization.Roles.Commands.RevokePermissionsFromRole;
using FAM.Application.Authorization.Roles.Commands.RevokeRoleFromUser;
using FAM.Application.Authorization.Roles.Commands.UpdateRole;
using FAM.Application.Authorization.Roles.Queries.GetRoleById;
using FAM.Application.Authorization.Roles.Shared;
using FAM.Application.Querying.Extensions;
using FAM.WebApi.Contracts.Authorization;
using FAM.WebApi.Contracts.Common;
using FAM.WebApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Role management controller
/// Manages roles and role assignments
/// </summary>
[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController : ControllerBase
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
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles([FromQuery] PaginationQueryParameters parameters)
    {
        var query = RoleMappers.ToQuery(parameters.ToQueryRequest());
        var result = await _mediator.Send(query);

        // Apply field selection if requested
        var fields = parameters.GetFieldsArray();
        if (fields != null && fields.Length > 0)
        {
            var selectedResult = result.SelectFieldsToResponse(fields);
            return Ok(selectedResult);
        }

        return Ok(result.ToPagedResponse());
    }

    /// <summary>
    /// Get role by ID with permissions
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(RoleWithPermissionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleWithPermissionsDto>> GetRoleById(long id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    [HttpPatch("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        return Ok();
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteRole(long id)
    {
        var command = new DeleteRoleCommand(id);
        await _mediator.Send(command);

        _logger.LogInformation("Role deleted: {RoleId}", id);

        return NoContent();
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

        return Ok();
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

        return Ok();
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

        return Ok(new { addedCount, message = $"Successfully added {addedCount} user(s) to role" });
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

        return Ok(new { removedCount, message = $"Successfully removed {removedCount} user(s) from role" });
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

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("User ID not found in claims");
    }
}