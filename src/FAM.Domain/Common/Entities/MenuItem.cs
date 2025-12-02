namespace FAM.Domain.Common.Entities;

/// <summary>
/// Menu item for UI navigation
/// Supports hierarchical structure (parent-child) and sorting
/// </summary>
public class MenuItem : BaseEntity
{
    /// <summary>
    /// Unique code for the menu item (e.g., "dashboard", "assets", "settings")
    /// Used for frontend routing and permission mapping
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Display name (default English)
    /// Frontend can use Code for i18n translation
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Icon name/class (e.g., "dashboard", "settings", "fa-home")
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// Route path (e.g., "/dashboard", "/assets/list")
    /// Null for parent menu items that only contain children
    /// </summary>
    public string? Route { get; private set; }

    /// <summary>
    /// External URL (for links that open in new tab)
    /// </summary>
    public string? ExternalUrl { get; private set; }

    /// <summary>
    /// Parent menu ID (null for root level items)
    /// </summary>
    public long? ParentId { get; private set; }

    /// <summary>
    /// Parent menu item
    /// </summary>
    public MenuItem? Parent { get; private set; }

    /// <summary>
    /// Child menu items
    /// </summary>
    public ICollection<MenuItem> Children { get; private set; } = new List<MenuItem>();

    /// <summary>
    /// Sort order within the same level (lower = first)
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Menu level (0 = root, 1 = first level child, etc.)
    /// Auto-calculated based on parent
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// Is the menu item visible?
    /// </summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Is the menu item enabled? (disabled items are shown but not clickable)
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Required permission code to view this menu item
    /// Null means publicly visible (no permission required)
    /// </summary>
    public string? RequiredPermission { get; private set; }

    /// <summary>
    /// Required roles (comma-separated) to view this menu item
    /// Null means all roles can see
    /// </summary>
    public string? RequiredRoles { get; private set; }

    /// <summary>
    /// Open in new tab?
    /// </summary>
    public bool OpenInNewTab { get; private set; }

    /// <summary>
    /// Custom CSS class for styling
    /// </summary>
    public string? CssClass { get; private set; }

    /// <summary>
    /// Badge text (e.g., "New", "Beta", count)
    /// </summary>
    public string? Badge { get; private set; }

    /// <summary>
    /// Badge color/variant (e.g., "primary", "danger", "success")
    /// </summary>
    public string? BadgeVariant { get; private set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; private set; }

    // Required for EF
    private MenuItem()
    {
    }

    /// <summary>
    /// Create a new menu item
    /// </summary>
    public static MenuItem Create(
        string code,
        string name,
        string? description = null,
        string? icon = null,
        string? route = null,
        long? parentId = null,
        int sortOrder = 0,
        string? requiredPermission = null,
        string? requiredRoles = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(ErrorCodes.VAL_REQUIRED, "Menu code is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ErrorCodes.VAL_REQUIRED, "Menu name is required");

        return new MenuItem
        {
            Code = code.ToLowerInvariant().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Icon = icon?.Trim(),
            Route = route?.Trim(),
            ParentId = parentId,
            SortOrder = sortOrder,
            Level = 0, // Will be calculated when parent is set
            IsVisible = true,
            IsEnabled = true,
            RequiredPermission = requiredPermission?.Trim(),
            RequiredRoles = requiredRoles?.Trim(),
            OpenInNewTab = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update menu item details
    /// </summary>
    public void Update(
        string name,
        string? description = null,
        string? icon = null,
        string? route = null,
        string? requiredPermission = null,
        string? requiredRoles = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ErrorCodes.VAL_REQUIRED, "Menu name is required");

        Name = name.Trim();
        Description = description?.Trim();
        Icon = icon?.Trim();
        Route = route?.Trim();
        RequiredPermission = requiredPermission?.Trim();
        RequiredRoles = requiredRoles?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set parent menu item
    /// </summary>
    public void SetParent(MenuItem? parent)
    {
        if (parent != null && parent.Id == Id)
            throw new DomainException(ErrorCodes.GEN_INVALID_OPERATION, "Menu item cannot be its own parent");

        // Check for circular reference
        if (parent != null && IsDescendantOf(parent))
            throw new DomainException(ErrorCodes.ORG_CIRCULAR_REFERENCE, "Circular reference detected");

        Parent = parent;
        ParentId = parent?.Id;
        Level = parent != null ? parent.Level + 1 : 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if this menu is a descendant of the given menu
    /// </summary>
    private bool IsDescendantOf(MenuItem potentialAncestor)
    {
        var current = Parent;
        while (current != null)
        {
            if (current.Id == potentialAncestor.Id)
                return true;
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Update sort order
    /// </summary>
    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Show menu item
    /// </summary>
    public void Show()
    {
        IsVisible = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Hide menu item
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enable menu item
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disable menu item
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set external URL
    /// </summary>
    public void SetExternalUrl(string? url, bool openInNewTab = true)
    {
        ExternalUrl = url?.Trim();
        OpenInNewTab = openInNewTab;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set badge
    /// </summary>
    public void SetBadge(string? text, string? variant = "primary")
    {
        Badge = text?.Trim();
        BadgeVariant = variant?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear badge
    /// </summary>
    public void ClearBadge()
    {
        Badge = null;
        BadgeVariant = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set CSS class
    /// </summary>
    public void SetCssClass(string? cssClass)
    {
        CssClass = cssClass?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set metadata JSON
    /// </summary>
    public void SetMetadata(string? metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if user has permission to view this menu
    /// </summary>
    public bool CanView(IEnumerable<string>? userPermissions, IEnumerable<string>? userRoles)
    {
        if (!IsVisible) return false;

        // Check required permission
        if (!string.IsNullOrEmpty(RequiredPermission))
            if (userPermissions == null || !userPermissions.Contains(RequiredPermission))
                return false;

        // Check required roles
        if (!string.IsNullOrEmpty(RequiredRoles))
        {
            var requiredRoleList = RequiredRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim());
            if (userRoles == null || !requiredRoleList.Any(r => userRoles.Contains(r)))
                return false;
        }

        return true;
    }
}