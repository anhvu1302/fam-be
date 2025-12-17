using System.Linq.Expressions;

using FAM.Application.Querying;
using FAM.Application.Querying.Validation;
using FAM.Domain.Common.Entities;

namespace FAM.Application.Settings.Shared;

/// <summary>
/// Field map for SystemSetting domain entity - defines allowed fields for filtering and sorting
/// </summary>
public class SystemSettingFieldMap : BaseFieldMap<SystemSetting>
{
    private static readonly Lazy<SystemSettingFieldMap> _instance = new(() => new SystemSettingFieldMap());

    private SystemSettingFieldMap()
    {
    }

    public static SystemSettingFieldMap Instance => _instance.Value;

    public override FieldMap<SystemSetting> Fields { get; } = new FieldMap<SystemSetting>()
        .Add("id", s => s.Id)
        .Add("key", s => s.Key)
        .Add("value", s => s.Value!)
        .Add("defaultValue", s => s.DefaultValue!)
        .Add("dataType", s => s.DataType)
        .Add("group", s => s.Group)
        .Add("displayName", s => s.DisplayName)
        .Add("description", s => s.Description!)
        .Add("sortOrder", s => s.SortOrder)
        .Add("isVisible", s => s.IsVisible)
        .Add("isEditable", s => s.IsEditable)
        .Add("isSensitive", s => s.IsSensitive)
        .Add("isRequired", s => s.IsRequired)
        .Add("createdAt", s => s.CreatedAt)
        .Add("updatedAt", s => s.UpdatedAt!);

    protected override Dictionary<string, Expression<Func<SystemSetting, object>>> AllowedIncludes { get; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // SystemSetting doesn't have related entities to include
        };
}
