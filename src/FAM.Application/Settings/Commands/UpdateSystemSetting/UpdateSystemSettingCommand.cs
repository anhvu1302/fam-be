using MediatR;

namespace FAM.Application.Settings.Commands.UpdateSystemSetting;

/// <summary>
/// Command to update a system setting
/// </summary>
public sealed record UpdateSystemSettingCommand : IRequest<Unit>
{
    public long Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? DefaultValue { get; init; }
    public string? Description { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsVisible { get; init; }
    public bool? IsEditable { get; init; }
    public string? ValidationRules { get; init; }
    public string? Options { get; init; }
}
