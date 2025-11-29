using FAM.Domain.Common.Entities;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository for MenuItem management
/// </summary>
public interface IMenuItemRepository : IRepository<MenuItem>
{
    /// <summary>
    /// Get menu item by code
    /// </summary>
    Task<MenuItem?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all root level menu items (no parent)
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetRootMenusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get menu items by parent ID
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetByParentIdAsync(long parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get full menu tree (all items with children loaded, max depth)
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetMenuTreeAsync(int maxDepth = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get visible menu tree for specific permissions and roles
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetVisibleMenuTreeAsync(
        IEnumerable<string>? userPermissions,
        IEnumerable<string>? userRoles,
        int maxDepth = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if code exists
    /// </summary>
    Task<bool> CodeExistsAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if menu has children
    /// </summary>
    Task<bool> HasChildrenAsync(long menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update sort orders for multiple menu items
    /// </summary>
    Task UpdateSortOrdersAsync(Dictionary<long, int> sortOrders, CancellationToken cancellationToken = default);
}
