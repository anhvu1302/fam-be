using System.Security.Claims;
using FAM.Application.Menu.DTOs;
using FAM.Application.Menu.Services;
using FAM.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Controller for Menu Item management
/// </summary>
[ApiController]
[Route("api/menus")]
public class MenusController : ControllerBase
{
    private readonly IMenuItemService _menuService;
    private readonly ILogger<MenusController> _logger;

    public MenusController(IMenuItemService menuService, ILogger<MenusController> logger)
    {
        _menuService = menuService;
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
        var menus = await _menuService.GetMenuTreeAsync(maxDepth, cancellationToken);
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

        var menus = await _menuService.GetVisibleMenuTreeAsync(permissions, roles, maxDepth, cancellationToken);
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
        var menus = await _menuService.GetAllAsync(cancellationToken);
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
        var menu = await _menuService.GetByIdAsync(id, cancellationToken);
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
        var menu = await _menuService.GetByCodeAsync(code, cancellationToken);
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
        var menu = await _menuService.CreateAsync(request, cancellationToken);
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
        var menu = await _menuService.UpdateAsync(id, request, cancellationToken);
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
        await _menuService.DeleteAsync(id, cancellationToken);
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
        await _menuService.UpdateSortOrdersAsync(request, cancellationToken);
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
        var menu = await _menuService.MoveAsync(id, parentId, sortOrder, cancellationToken);
        return Ok(menu);
    }

    #endregion
}