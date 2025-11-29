using FAM.Domain.Common.Entities;

namespace FAM.Application.Settings.DTOs;

#region Request DTOs

/// <summary>
/// Request to create a system setting
/// </summary>
public record CreateSystemSettingRequest(
    string Key,
    string DisplayName,
    string? Value = null,
    string? DefaultValue = null,
    SettingDataType DataType = SettingDataType.String,
    string Group = "general",
    string? Description = null,
    int SortOrder = 0,
    bool IsRequired = false,
    bool IsSensitive = false,
    bool IsVisible = true,
    bool IsEditable = true,
    string? ValidationRules = null,
    string? Options = null);

/// <summary>
/// Request to update a system setting
/// </summary>
public record UpdateSystemSettingRequest(
    string DisplayName,
    string? Value = null,
    string? DefaultValue = null,
    string? Description = null,
    int? SortOrder = null,
    bool? IsVisible = null,
    bool? IsEditable = null,
    string? ValidationRules = null,
    string? Options = null);

/// <summary>
/// Request to update setting value only
/// </summary>
public record UpdateSettingValueRequest(string? Value);

/// <summary>
/// Request to bulk update settings
/// </summary>
public record BulkUpdateSettingsRequest(Dictionary<string, string?> Settings);

#endregion

#region Response DTOs

/// <summary>
/// System setting response
/// </summary>
public record SystemSettingResponse(
    long Id,
    string Key,
    string? Value,
    string? DefaultValue,
    string? EffectiveValue,
    SettingDataType DataType,
    string Group,
    string DisplayName,
    string? Description,
    int SortOrder,
    bool IsVisible,
    bool IsEditable,
    bool IsSensitive,
    bool IsRequired,
    string? ValidationRules,
    string? Options,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static SystemSettingResponse FromDomain(SystemSetting setting, bool maskSensitive = true)
    {
        var value = setting.Value;
        var effectiveValue = setting.GetEffectiveValue();
        
        // Mask sensitive values
        if (maskSensitive && setting.IsSensitive && !string.IsNullOrEmpty(value))
        {
            value = "********";
            effectiveValue = "********";
        }

        return new SystemSettingResponse(
            setting.Id,
            setting.Key,
            value,
            setting.DefaultValue,
            effectiveValue,
            setting.DataType,
            setting.Group,
            setting.DisplayName,
            setting.Description,
            setting.SortOrder,
            setting.IsVisible,
            setting.IsEditable,
            setting.IsSensitive,
            setting.IsRequired,
            setting.ValidationRules,
            setting.Options,
            setting.CreatedAt,
            setting.UpdatedAt);
    }
}

/// <summary>
/// Public setting response (for caching by FE, no sensitive info)
/// </summary>
public record PublicSettingResponse(
    string Key,
    string? Value,
    SettingDataType DataType,
    string Group)
{
    public static PublicSettingResponse FromDomain(SystemSetting setting)
    {
        var value = setting.IsSensitive ? null : setting.GetEffectiveValue();
        
        return new PublicSettingResponse(
            setting.Key,
            value,
            setting.DataType,
            setting.Group);
    }
}

/// <summary>
/// Settings grouped by category
/// </summary>
public record SettingsGroupResponse(
    string Group,
    List<SystemSettingResponse> Settings);

/// <summary>
/// Public settings grouped by category
/// </summary>
public record PublicSettingsGroupResponse(
    string Group,
    List<PublicSettingResponse> Settings);

#endregion
