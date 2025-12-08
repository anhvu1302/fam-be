using FAM.Application.Authorization.Permissions.Queries.GetPermissions;
using FAM.Application.Authorization.Permissions.Shared;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using FAM.Application.Settings;
using FAM.Domain.Authorization;
using FAM.WebApi.Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Permission management controller
/// Manages system permissions
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;
    private readonly PaginationSettings _pagination;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger,
        PaginationSettings pagination)
    {
        _mediator = mediator;
        _logger = logger;
        _pagination = pagination;
    }

    /// <summary>
    /// Get paginated list of permissions with advanced filtering and sorting
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of permissions. Supports filtering, sorting, field selection, and includes.
    /// 
    /// Query Parameters:
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 10)
    /// - sortBy: Field to sort by (e.g., "id", "resource", "action")
    /// - sortOrder: Sort order - "asc" or "desc" (default: asc)
    /// - fields: Comma-separated fields to include in response
    /// - search: Search keyword for filtering permissions
    /// 
    /// Example: GET /api/permissions?page=1&amp;pageSize=10&amp;sortBy=resource&amp;sortOrder=asc
    /// </remarks>
    /// <param name="parameters">Query parameters for pagination, filtering, sorting, field selection and includes</param>
    /// <response code="200">Success - Returns {success: true, result: {items: [Permission], totalCount, pageNumber, pageSize, totalPages}}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid query parameters", code: "INVALID_PARAMETERS"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<PageResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPermissions([FromQuery] PaginationQueryParameters parameters)
    {
        var query = new GetPermissionsQuery(parameters.ToQueryRequest());
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
    /// Get all available permission definitions
    /// </summary>
    /// <remarks>
    /// Returns a list of all predefined permissions in the system.
    /// Each permission includes: Resource, Action, Description, and PermissionKey
    /// 
    /// Example: GET /api/permissions/definitions
    /// 
    /// Permissions are used to control access to specific resources and actions.
    /// Format: {Resource}:{Action}
    /// </remarks>
    /// <response code="200">Success - Returns {success: true, result: [{Resource, Action, Description, PermissionKey}, ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IReadOnlyList<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult GetPermissionDefinitions()
    {
        var permissions = Permissions.All
            .Select(p => new
            {
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description,
                PermissionKey = $"{p.Resource}:{p.Action}"
            })
            .ToList();

        return OkResponse(permissions);
    }

    /// <summary>
    /// Get available resources
    /// </summary>
    /// <remarks>
    /// Returns a list of all available resources in the system.
    /// Resources are entities that have access control (e.g., Users, Roles, Assets, etc.)
    /// 
    /// Example: GET /api/permissions/resources
    /// </remarks>
    /// <response code="200">Success - Returns {success: true, result: ["Users", "Roles", "Assets", ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("resources")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult GetResources()
    {
        var resources = Resources.All;
        return OkResponse(resources);
    }

    /// <summary>
    /// Get available actions
    /// </summary>
    /// <remarks>
    /// Returns a list of all available actions in the system.
    /// Actions are operations that can be performed on resources (e.g., Create, Read, Update, Delete)
    /// 
    /// Example: GET /api/permissions/actions
    /// </remarks>
    /// <response code="200">Success - Returns {success: true, result: ["Create", "Read", "Update", "Delete", ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("actions")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult GetActions()
    {
        var actions = Actions.All;
        return OkResponse(actions);
    }
}