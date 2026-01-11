using FAM.Domain.Common.Entities;

namespace FAM.Application.Settings.Shared;

public static class SystemSettingResponseMappers
{
    /// <summary>
    /// Convert Domain SystemSetting entity to SystemSettingDto
    /// </summary>
    public static SystemSettingDto? ToDto(this SystemSetting? setting)
    {
        if (setting == null)
        {
            return null;
        }

        return new SystemSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            DefaultValue = setting.DefaultValue,
            EffectiveValue = setting.GetEffectiveValue(),
            DataType = setting.DataType,
            Group = setting.Group,
            DisplayName = setting.DisplayName,
            Description = setting.Description,
            SortOrder = setting.SortOrder,
            IsVisible = setting.IsVisible,
            IsEditable = setting.IsEditable,
            IsSensitive = setting.IsSensitive,
            IsRequired = setting.IsRequired,
            ValidationRules = setting.ValidationRules,
            Options = setting.Options,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt
        };
    }

    /// <summary>
    /// Convert to PublicSettingDto (excluding sensitive data)
    /// </summary>
    public static PublicSettingDto? ToPublicDto(this SystemSetting? setting)
    {
        if (setting == null || setting.IsSensitive)
        {
            return null;
        }

        return new PublicSettingDto
        {
            Key = setting.Key,
            Value = setting.GetEffectiveValue(),
            DataType = setting.DataType,
            Group = setting.Group,
            DisplayName = setting.DisplayName,
            Description = setting.Description
        };
    }
}
