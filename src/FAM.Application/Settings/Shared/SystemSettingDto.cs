using FAM.Domain.Common.Entities;

namespace FAM.Application.Settings.Shared;

/// <summary>
/// System setting DTO for API responses
/// </summary>
public sealed record SystemSettingDto
{
    public long Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? DefaultValue { get; init; }
    public string? EffectiveValue { get; init; }
    public SettingDataType DataType { get; init; }
    public string Group { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public bool IsVisible { get; init; }
    public bool IsEditable { get; init; }
    public bool IsSensitive { get; init; }
    public bool IsRequired { get; init; }
    public string? ValidationRules { get; init; }
    public string? Options { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Public setting DTO (without sensitive data)
/// </summary>
public sealed record PublicSettingDto
{
    public string Key { get; init; } = string.Empty;
    public string? Value { get; init; }
    public SettingDataType DataType { get; init; }
    public string Group { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
}
