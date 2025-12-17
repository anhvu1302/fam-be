using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Commands.CreateSystemSetting;

/// <summary>
/// Command to create a new system setting
/// </summary>
public sealed record CreateSystemSettingCommand : IRequest<long>
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? DefaultValue { get; init; }
    public SettingDataType DataType { get; init; } = SettingDataType.String;
    public string Group { get; init; } = "general";
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public bool IsRequired { get; init; }
    public bool IsSensitive { get; init; }
    public bool IsVisible { get; init; } = true;
    public bool IsEditable { get; init; } = true;
    public string? ValidationRules { get; init; }
    public string? Options { get; init; }
}
