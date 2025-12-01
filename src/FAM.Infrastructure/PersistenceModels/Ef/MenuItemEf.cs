using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for MenuItem
/// </summary>
[Table("menu_items")]
public class MenuItemEf : BaseEntityEf
{
    /// <summary>
    /// Unique code for the menu item
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon name/class
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Route path
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// External URL
    /// </summary>
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// Parent menu ID
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Parent menu item
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public MenuItemEf? Parent { get; set; }

    /// <summary>
    /// Child menu items
    /// </summary>
    public ICollection<MenuItemEf> Children { get; set; } = new List<MenuItemEf>();

    /// <summary>
    /// Sort order
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Menu level (0 = root)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Required permission code
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Required roles (comma-separated)
    /// </summary>
    public string? RequiredRoles { get; set; }

    /// <summary>
    /// Open in new tab
    /// </summary>
    public bool OpenInNewTab { get; set; }

    /// <summary>
    /// Custom CSS class
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Badge text
    /// </summary>
    public string? Badge { get; set; }

    /// <summary>
    /// Badge variant
    /// </summary>
    public string? BadgeVariant { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }
}