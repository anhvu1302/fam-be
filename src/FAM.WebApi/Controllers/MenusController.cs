using System.Security.Claims;

using FAM.Application.Menu.Commands.CreateMenu;
using FAM.Application.Menu.Commands.DeleteMenu;
using FAM.Application.Menu.Commands.MoveMenu;
using FAM.Application.Menu.Commands.UpdateMenu;
using FAM.Application.Menu.Commands.UpdateMenuSortOrders;
using FAM.Application.Menu.DTOs;
using FAM.Application.Menu.Queries.GetAllMenus;
using FAM.Application.Menu.Queries.GetMenuByCode;
using FAM.Application.Menu.Queries.GetMenuById;
using FAM.Application.Menu.Queries.GetMenuTree;
using FAM.Application.Menu.Queries.GetVisibleMenuTree;
using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Common;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Controller for Menu Item management
/// </summary>
[ApiController]
[Route("api/menus")]
public class MenusController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<MenusController> _logger;

    public MenusController(IMediator mediator, ILogger<MenusController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Public Endpoints

    /// <summary>
    /// Get full menu tree for navigation (public endpoint)
    /// </summary>
    /// <remarks>
    /// Returns all root menus with children up to specified depth level.
    /// This endpoint is publicly accessible without authentication.
    /// 
    /// Example: GET /api/menus/tree?maxDepth=3
    /// </remarks>
    /// <param name="maxDepth">Maximum depth level for nested menus (default: 3)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: [MenuItemResponse, ...]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<MenuItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuTree(
        [FromQuery] int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMenuTreeQuery(maxDepth);
        IEnumerable<MenuItemResponse> menus = await _mediator.Send(query, cancellationToken);
        return OkResponse(menus);
    }

    /// <summary>
    /// Get visible menu tree for current user (based on permissions/roles)
    /// </summary>
    /// <remarks>
    /// Returns menu tree filtered by user's permissions and roles.
    /// Requires authentication. Only shows menus the user has access to.
    /// 
    /// Example: GET /api/menus/tree/visible?maxDepth=3
    /// </remarks>
    /// <param name="maxDepth">Maximum depth level for nested menus (default: 3)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: [MenuItemResponse, ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("tree/visible")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<MenuItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetVisibleMenuTree(
        [FromQuery] int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        // Get user permissions and roles from claims
        var permissions = User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var query = new GetVisibleMenuTreeQuery(permissions, roles, maxDepth);
        IEnumerable<MenuItemResponse> menus = await _mediator.Send(query, cancellationToken);
        return OkResponse(menus);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all menu items as flat list (admin only)
    /// </summary>
    /// <remarks>
    /// Returns all menu items in a flat list format. Admin/SuperAdmin only.
    /// Useful for management and editing purposes.
    /// 
    /// Example: GET /api/menus
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: [MenuItemFlatResponse, ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<MenuItemFlatResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MenuItemFlatResponse>>> GetAllMenus(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllMenusQuery();
        IEnumerable<MenuItemFlatResponse> menus = await _mediator.Send(query, cancellationToken);
        return OkResponse(menus);
    }

    /// <summary>
    /// Get a specific menu item by ID
    /// </summary>
    /// <remarks>
    /// Returns a specific menu item with all its properties. Admin/SuperAdmin only.
    /// 
    /// Example: GET /api/menus/1
    /// </remarks>
    /// <param name="id">Menu item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: MenuItemResponse}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Menu item not found", code: "MENU_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MenuItemResponse>> GetMenuById(long id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMenuByIdQuery(id);
        MenuItemResponse? menu = await _mediator.Send(query, cancellationToken);
        if (menu == null)
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", id);
        return OkResponse(menu);
    }

    /// <summary>
    /// Get a specific menu item by code
    /// </summary>
    /// <remarks>
    /// Returns a specific menu item using its unique code identifier. Admin/SuperAdmin only.
    /// 
    /// Example: GET /api/menus/code/DASHBOARD
    /// </remarks>
    /// <param name="code">Menu item code (unique identifier)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: MenuItemResponse}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Menu item with code {code} not found", code: "MENU_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("code/{code}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MenuItemResponse>> GetMenuByCode(string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMenuByCodeQuery(code);
        MenuItemResponse? menu = await _mediator.Send(query, cancellationToken);
        if (menu == null)
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", code);
        return OkResponse(menu);
    }

    /// <summary>
    /// Create a new menu item
    /// </summary>
    /// <remarks>
    /// Creates a new menu item with specified properties. Admin/SuperAdmin only.
    /// Can be used to create navigation menus for the application.
    /// 
    /// Request body:
    /// {
    ///   "code": "DASHBOARD",
    ///   "name": "Dashboard",
    ///   "description": "Main dashboard",
    ///   "icon": "dashboard",
    ///   "route": "/dashboard",
    ///   "externalUrl": null,
    ///   "parentId": null,
    ///   "sortOrder": 1,
    ///   "requiredPermission": "Dashboard:View",
    ///   "requiredRoles": ["Admin", "Manager"],
    ///   "openInNewTab": false
    /// }
    /// 
    /// Example: POST /api/menus
    /// </remarks>
    /// <param name="request">CreateMenuItemRequest with menu details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="201">Created - Returns {success: true, result: MenuItemResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid menu data", code: "INVALID_MENU_DATA"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MenuItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MenuItemResponse>> CreateMenu(
        [FromBody] CreateMenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateMenuCommand(
            request.Code,
            request.Name,
            request.Description,
            request.Icon,
            request.Route,
            request.ExternalUrl,
            request.ParentId,
            request.SortOrder,
            request.RequiredPermission,
            request.RequiredRoles,
            request.OpenInNewTab);
        MenuItemResponse menu = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMenuById), new { id = menu.Id }, menu);
    }

    /// <summary>
    /// Update a menu item
    /// </summary>
    /// <remarks>
    /// Updates an existing menu item with new values. Admin/SuperAdmin only.
    /// 
    /// Request body: Same structure as CreateMenuItemRequest
    /// 
    /// Example: PUT /api/menus/1
    /// </remarks>
    /// <param name="id">Menu item ID to update</param>
    /// <param name="request">UpdateMenuItemRequest with menu details to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: MenuItemResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid menu data", code: "INVALID_MENU_DATA"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Menu item not found", code: "MENU_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MenuItemResponse>> UpdateMenu(
        long id,
        [FromBody] UpdateMenuItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateMenuCommand(
            id,
            request.Name,
            request.Description,
            request.Icon,
            request.Route,
            request.ExternalUrl,
            request.ParentId,
            request.SortOrder,
            request.RequiredPermission,
            request.RequiredRoles,
            request.OpenInNewTab,
            request.IsVisible,
            request.IsEnabled,
            request.CssClass,
            request.Badge,
            request.BadgeVariant);
        MenuItemResponse menu = await _mediator.Send(command, cancellationToken);
        return OkResponse(menu);
    }

    /// <summary>
    /// Delete a menu item
    /// </summary>
    /// <remarks>
    /// Deletes a menu item and its children. Admin/SuperAdmin only.
    /// Cannot be undone. Use with caution.
    /// 
    /// Example: DELETE /api/menus/1
    /// </remarks>
    /// <param name="id">Menu item ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Success - No content returned</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Cannot delete root menu", code: "INVALID_OPERATION"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Menu item not found", code: "MENU_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMenu(long id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteMenuCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update sort orders for multiple menu items
    /// </summary>
    /// <remarks>
    /// Updates the sort order for multiple menu items at once. Admin/SuperAdmin only.
    /// Useful for reorganizing menu structure.
    /// 
    /// Request body:
    /// {
    ///   "sortOrders": [
    ///     {"id": 1, "sortOrder": 1},
    ///     {"id": 2, "sortOrder": 2}
    ///   ]
    /// }
    /// 
    /// Example: PUT /api/menus/sort-orders
    /// </remarks>
    /// <param name="request">UpdateMenuSortOrdersRequest with new sort orders</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Success - No content returned</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid sort order data", code: "INVALID_SORT_ORDER"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPut("sort-orders")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSortOrders(
        [FromBody] UpdateMenuSortOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateMenuSortOrdersCommand(request.SortOrders);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Move a menu item to new parent
    /// </summary>
    /// <remarks>
    /// Moves a menu item to a new parent with new sort order. Admin/SuperAdmin only.
    /// 
    /// Query parameters:
    /// - parentId: New parent ID (null for root level)
    /// - sortOrder: New sort order position (default: 0)
    /// 
    /// Example: PUT /api/menus/5/move?parentId=1&amp;sortOrder=2
    /// </remarks>
    /// <param name="id">Menu item ID to move</param>
    /// <param name="parentId">New parent ID (null for root level)</param>
    /// <param name="sortOrder">New sort order position</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Success - Returns {success: true, result: MenuItemResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Cannot move menu to itself", code: "INVALID_OPERATION"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: "User not authorized", code: "FORBIDDEN"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Menu item not found", code: "MENU_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPut("{id:long}/move")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MenuItemResponse>> MoveMenu(
        long id,
        [FromQuery] long? parentId,
        [FromQuery] int sortOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var command = new MoveMenuCommand(id, parentId, sortOrder);
        MenuItemResponse menu = await _mediator.Send(command, cancellationToken);
        return OkResponse(menu);
    }

    #endregion
}
