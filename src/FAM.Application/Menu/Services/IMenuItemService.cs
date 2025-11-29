using FAM.Application.Menu.DTOs;

namespace FAM.Application.Menu.Services;

/// <summary>
/// Service for menu item management
/// </summary>
public interface IMenuItemService
{
    /// <summary>
    /// Get all menu items as flat list
    /// </summary>
    Task<IEnumerable<MenuItemFlatResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get menu item by ID
    /// </summary>
    Task<MenuItemResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get menu item by code
    /// </summary>
    Task<MenuItemResponse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get full menu tree (root items with all children up to maxDepth)
    /// </summary>
    Task<IEnumerable<MenuItemResponse>> GetMenuTreeAsync(int maxDepth = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get visible menu tree for user permissions/roles
    /// </summary>
    Task<IEnumerable<MenuItemResponse>> GetVisibleMenuTreeAsync(
        IEnumerable<string>? userPermissions,
        IEnumerable<string>? userRoles,
        int maxDepth = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new menu item
    /// </summary>
    Task<MenuItemResponse> CreateAsync(CreateMenuItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a menu item
    /// </summary>
    Task<MenuItemResponse> UpdateAsync(long id, UpdateMenuItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a menu item
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update sort orders for multiple menu items
    /// </summary>
    Task UpdateSortOrdersAsync(UpdateMenuSortOrdersRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move menu item to new parent
    /// </summary>
    Task<MenuItemResponse> MoveAsync(long id, long? newParentId, int sortOrder, CancellationToken cancellationToken = default);
}
