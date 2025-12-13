using FAM.Domain.Common.Entities;

namespace FAM.Application.Menu.DTOs;

#region Request DTOs

/// <summary>
/// Request to create a menu item
/// </summary>
public record CreateMenuItemRequest(
    string Code,
    string Name,
    string? Description = null,
    string? Icon = null,
    string? Route = null,
    string? ExternalUrl = null,
    long? ParentId = null,
    int SortOrder = 0,
    string? RequiredPermission = null,
    string? RequiredRoles = null,
    bool OpenInNewTab = false);

/// <summary>
/// Request to update a menu item
/// </summary>
public record UpdateMenuItemRequest(
    string Name,
    string? Description = null,
    string? Icon = null,
    string? Route = null,
    string? ExternalUrl = null,
    long? ParentId = null,
    int? SortOrder = null,
    string? RequiredPermission = null,
    string? RequiredRoles = null,
    bool? OpenInNewTab = null,
    bool? IsVisible = null,
    bool? IsEnabled = null,
    string? CssClass = null,
    string? Badge = null,
    string? BadgeVariant = null);

/// <summary>
/// Request to update sort orders
/// </summary>
public record UpdateMenuSortOrdersRequest(Dictionary<long, int> SortOrders);

#endregion

#region Response DTOs

/// <summary>
/// Menu item response with children (tree structure)
/// </summary>
public record MenuItemResponse(
    long Id,
    string Code,
    string Name,
    string? Description,
    string? Icon,
    string? Route,
    string? ExternalUrl,
    long? ParentId,
    int SortOrder,
    int Level,
    bool IsVisible,
    bool IsEnabled,
    string? RequiredPermission,
    string? RequiredRoles,
    bool OpenInNewTab,
    string? CssClass,
    string? Badge,
    string? BadgeVariant,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<MenuItemResponse>? Children = null)
{
    public static MenuItemResponse FromDomain(MenuItem menu, bool includeChildren = true, int maxDepth = 3,
        int currentDepth = 0)
    {
        List<MenuItemResponse>? children = null;

        if (includeChildren && menu.Children.Any() && currentDepth < maxDepth)
            children = menu.Children
                .OrderBy(c => c.SortOrder)
                .Select(c => FromDomain(c, true, maxDepth, currentDepth + 1))
                .ToList();

        return new MenuItemResponse(
            menu.Id,
            menu.Code,
            menu.Name,
            menu.Description,
            menu.Icon,
            menu.Route,
            menu.ExternalUrl,
            menu.ParentId,
            menu.SortOrder,
            menu.Level,
            menu.IsVisible,
            menu.IsEnabled,
            menu.RequiredPermission,
            menu.RequiredRoles,
            menu.OpenInNewTab,
            menu.CssClass,
            menu.Badge,
            menu.BadgeVariant,
            menu.CreatedAt,
            menu.UpdatedAt,
            children);
    }
}

/// <summary>
/// Flat menu item response (no children)
/// </summary>
public record MenuItemFlatResponse(
    long Id,
    string Code,
    string Name,
    string? Description,
    string? Icon,
    string? Route,
    string? ExternalUrl,
    long? ParentId,
    int SortOrder,
    int Level,
    bool IsVisible,
    bool IsEnabled,
    string? RequiredPermission,
    string? RequiredRoles,
    bool OpenInNewTab,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static MenuItemFlatResponse FromDomain(MenuItem menu)
    {
        return new MenuItemFlatResponse(
            menu.Id,
            menu.Code,
            menu.Name,
            menu.Description,
            menu.Icon,
            menu.Route,
            menu.ExternalUrl,
            menu.ParentId,
            menu.SortOrder,
            menu.Level,
            menu.IsVisible,
            menu.IsEnabled,
            menu.RequiredPermission,
            menu.RequiredRoles,
            menu.OpenInNewTab,
            menu.CreatedAt,
            menu.UpdatedAt);
    }
}

#endregion
