using System.Linq.Expressions;
using FAM.Application.Querying;
using FAM.Application.Querying.Validation;
using FAM.Domain.Authorization;

namespace FAM.Application.Authorization.Permissions.Shared;

/// <summary>
/// Field map for Permission domain entity - defines allowed fields for filtering and sorting
/// </summary>
public class PermissionFieldMap : BaseFieldMap<Permission>
{
    private static readonly Lazy<PermissionFieldMap> _instance = new(() => new PermissionFieldMap());

    private PermissionFieldMap()
    {
    }

    public static PermissionFieldMap Instance => _instance.Value;

    public override FieldMap<Permission> Fields { get; } = new FieldMap<Permission>()
        .Add("id", p => p.Id)
        .Add("resource", p => p.Resource.Value)
        .Add("action", p => p.Action.Value)
        .Add("description", p => p.Description!)
        .Add("createdAt", p => p.CreatedAt)
        .Add("updatedAt", p => p.UpdatedAt!)
        .Add("deletedAt", p => p.DeletedAt!);

    protected override Dictionary<string, Expression<Func<Permission, object>>> AllowedIncludes { get; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["rolepermissions"] = p => p.RolePermissions
        };
}