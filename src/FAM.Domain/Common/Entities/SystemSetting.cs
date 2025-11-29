using FAM.Domain.Common;

namespace FAM.Domain.Common.Entities;

/// <summary>
/// System setting for storing configuration values
/// Supports different data types and grouping
/// </summary>
public class SystemSetting : BaseEntity
{
    /// <summary>
    /// Setting key (unique identifier)
    /// Format: group.subgroup.name (e.g., "app.general.siteName", "app.footer.copyright")
    /// </summary>
    public string Key { get; private set; } = null!;

    /// <summary>
    /// Setting value (stored as string, can be parsed based on DataType)
    /// </summary>
    public string? Value { get; private set; }

    /// <summary>
    /// Default value if Value is null or empty
    /// </summary>
    public string? DefaultValue { get; private set; }

    /// <summary>
    /// Data type of the value
    /// </summary>
    public SettingDataType DataType { get; private set; } = SettingDataType.String;

    /// <summary>
    /// Group/category for organizing settings (e.g., "general", "footer", "branding", "email")
    /// </summary>
    public string Group { get; private set; } = "general";

    /// <summary>
    /// Display name for the setting
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Description of what this setting controls
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Sort order within group
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Is this setting visible in admin UI?
    /// </summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Is this setting editable by admin?
    /// </summary>
    public bool IsEditable { get; private set; } = true;

    /// <summary>
    /// Is this a sensitive setting? (value should be masked in UI)
    /// </summary>
    public bool IsSensitive { get; private set; }

    /// <summary>
    /// Is this setting required?
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Validation rules as JSON (e.g., {"min": 0, "max": 100}, {"pattern": "^https?://"})
    /// </summary>
    public string? ValidationRules { get; private set; }

    /// <summary>
    /// Options for select/dropdown type as JSON array
    /// e.g., [{"value": "light", "label": "Light"}, {"value": "dark", "label": "Dark"}]
    /// </summary>
    public string? Options { get; private set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Last modified by user ID
    /// </summary>
    public long? LastModifiedBy { get; private set; }

    // Required for EF
    private SystemSetting() { }

    /// <summary>
    /// Create a new system setting
    /// </summary>
    public static SystemSetting Create(
        string key,
        string displayName,
        string? value = null,
        string? defaultValue = null,
        SettingDataType dataType = SettingDataType.String,
        string group = "general",
        string? description = null,
        int sortOrder = 0,
        bool isRequired = false,
        bool isSensitive = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException(ErrorCodes.VAL_REQUIRED, "Setting key is required");
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException(ErrorCodes.VAL_REQUIRED, "Display name is required");

        return new SystemSetting
        {
            Key = key.ToLowerInvariant().Trim(),
            DisplayName = displayName.Trim(),
            Value = value,
            DefaultValue = defaultValue,
            DataType = dataType,
            Group = group.ToLowerInvariant().Trim(),
            Description = description?.Trim(),
            SortOrder = sortOrder,
            IsVisible = true,
            IsEditable = true,
            IsRequired = isRequired,
            IsSensitive = isSensitive,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update setting value
    /// </summary>
    public void SetValue(string? value, long? modifiedBy = null)
    {
        Value = value;
        LastModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get effective value (Value if set, otherwise DefaultValue)
    /// </summary>
    public string? GetEffectiveValue()
    {
        return !string.IsNullOrEmpty(Value) ? Value : DefaultValue;
    }

    /// <summary>
    /// Get value as boolean
    /// </summary>
    public bool GetBoolValue(bool defaultValue = false)
    {
        var val = GetEffectiveValue();
        if (string.IsNullOrEmpty(val)) return defaultValue;
        return bool.TryParse(val, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Get value as integer
    /// </summary>
    public int GetIntValue(int defaultValue = 0)
    {
        var val = GetEffectiveValue();
        if (string.IsNullOrEmpty(val)) return defaultValue;
        return int.TryParse(val, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Get value as decimal
    /// </summary>
    public decimal GetDecimalValue(decimal defaultValue = 0)
    {
        var val = GetEffectiveValue();
        if (string.IsNullOrEmpty(val)) return defaultValue;
        return decimal.TryParse(val, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Update setting details
    /// </summary>
    public void Update(
        string displayName,
        string? description = null,
        string? defaultValue = null,
        int? sortOrder = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException(ErrorCodes.VAL_REQUIRED, "Display name is required");

        DisplayName = displayName.Trim();
        Description = description?.Trim();
        DefaultValue = defaultValue;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set validation rules
    /// </summary>
    public void SetValidationRules(string? rules)
    {
        ValidationRules = rules;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set options for select type
    /// </summary>
    public void SetOptions(string? options)
    {
        Options = options;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Make setting visible
    /// </summary>
    public void Show()
    {
        IsVisible = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Hide setting from UI
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Make setting editable
    /// </summary>
    public void MakeEditable()
    {
        IsEditable = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Make setting read-only
    /// </summary>
    public void MakeReadOnly()
    {
        IsEditable = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Data types for system settings
/// </summary>
public enum SettingDataType
{
    /// <summary>Plain text string</summary>
    String = 0,
    
    /// <summary>Multi-line text</summary>
    Text = 1,
    
    /// <summary>Integer number</summary>
    Integer = 2,
    
    /// <summary>Decimal number</summary>
    Decimal = 3,
    
    /// <summary>Boolean (true/false)</summary>
    Boolean = 4,
    
    /// <summary>Date only</summary>
    Date = 5,
    
    /// <summary>Date and time</summary>
    DateTime = 6,
    
    /// <summary>JSON object</summary>
    Json = 7,
    
    /// <summary>URL</summary>
    Url = 8,
    
    /// <summary>Email address</summary>
    Email = 9,
    
    /// <summary>Color (hex)</summary>
    Color = 10,
    
    /// <summary>File path or URL (for images, files)</summary>
    File = 11,
    
    /// <summary>Image URL</summary>
    Image = 12,
    
    /// <summary>HTML content</summary>
    Html = 13,
    
    /// <summary>Select from predefined options</summary>
    Select = 14,
    
    /// <summary>Multiple select</summary>
    MultiSelect = 15,
    
    /// <summary>Password/Secret (will be encrypted)</summary>
    Secret = 16
}
