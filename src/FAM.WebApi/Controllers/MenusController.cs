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
using FAM.Domain.Common;
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
    /// Returns all root menus with children up to 3 levels deep
    /// </summary>
    [HttpGet("tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<MenuItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuTree(
        [FromQuery] int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMenuTreeQuery(maxDepth);
        var menus = await _mediator.Send(query, cancellationToken);
        return Ok(menus);
    }

    /// <summary>
    /// Get visible menu tree for current user (based on permissions/roles)
    /// </summary>
    [HttpGet("tree/visible")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<MenuItemResponse>), StatusCodes.Status200OK)]
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
        var menus = await _mediator.Send(query, cancellationToken);
        return Ok(menus);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all menu items as flat list (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<MenuItemFlatResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuItemFlatResponse>>> GetAllMenus(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllMenusQuery();
        var menus = await _mediator.Send(query, cancellationToken);
        return Ok(menus);
    }

    /// <summary>
    /// Get a specific menu item by ID
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItemResponse>> GetMenuById(long id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMenuByIdQuery(id);
        var menu = await _mediator.Send(query, cancellationToken);
        if (menu == null)
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", id);
        return Ok(menu);
    }

    /// <summary>
    /// Get a specific menu item by code
    /// </summary>
    [HttpGet("code/{code}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItemResponse>> GetMenuByCode(string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMenuByCodeQuery(code);
        var menu = await _mediator.Send(query, cancellationToken);
        if (menu == null)
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", code);
        return Ok(menu);
    }

    /// <summary>
    /// Create a new menu item
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        var menu = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMenuById), new { id = menu.Id }, menu);
    }

    /// <summary>
    /// Update a menu item
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        var menu = await _mediator.Send(command, cancellationToken);
        return Ok(menu);
    }

    /// <summary>
    /// Delete a menu item
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMenu(long id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteMenuCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update sort orders for multiple menu items
    /// </summary>
    [HttpPut("sort-orders")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [HttpPut("{id:long}/move")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MenuItemResponse>> MoveMenu(
        long id,
        [FromQuery] long? parentId,
        [FromQuery] int sortOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var command = new MoveMenuCommand(id, parentId, sortOrder);
        var menu = await _mediator.Send(command, cancellationToken);
        return Ok(menu);
    }

    #endregion
}