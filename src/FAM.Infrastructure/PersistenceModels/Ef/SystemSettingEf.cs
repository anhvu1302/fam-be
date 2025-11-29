using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for SystemSetting
/// </summary>
[Table("system_settings")]
public class SystemSettingEf : BaseEntityEf
{
    /// <summary>
    /// Setting key (unique)
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Setting value
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Default value
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Data type
    /// </summary>
    public int DataType { get; set; }

    /// <summary>
    /// Group/category
    /// </summary>
    public string Group { get; set; } = "general";

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sort order within group
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Is visible in admin UI
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Is editable
    /// </summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Is sensitive (value should be masked)
    /// </summary>
    public bool IsSensitive { get; set; }

    /// <summary>
    /// Is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Validation rules as JSON
    /// </summary>
    public string? ValidationRules { get; set; }

    /// <summary>
    /// Options for select type as JSON
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Last modified by user ID
    /// </summary>
    public long? LastModifiedBy { get; set; }
}
