using System.Linq.Expressions;

using FAM.Application.Querying;
using FAM.Application.Querying.Validation;
using FAM.Domain.Authorization;

namespace FAM.Application.Authorization.Roles.Shared;

/// <summary>
/// Field map for Role domain entity - defines allowed fields for filtering and sorting
/// </summary>
public class RoleFieldMap : BaseFieldMap<Role>
{
    private static readonly Lazy<RoleFieldMap> _instance = new(() => new RoleFieldMap());

    private RoleFieldMap()
    {
    }

    public static RoleFieldMap Instance => _instance.Value;

    public override FieldMap<Role> Fields { get; } = new FieldMap<Role>()
        .Add("id", r => r.Id)
        .Add("code", r => r.Code)
        .Add("name", r => r.Name)
        .Add("description", r => r.Description!)
        .Add("rank", r => r.Rank)
        .Add("isSystemRole", r => r.IsSystemRole)
        .Add("createdAt", r => r.CreatedAt)
        .Add("updatedAt", r => r.UpdatedAt!)
        .Add("deletedAt", r => r.DeletedAt!);

    protected override Dictionary<string, Expression<Func<Role, object>>> AllowedIncludes { get; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["rolepermissions"] = r => r.RolePermissions,
            ["usernoderoles"] = r => r.UserNodeRoles
        };
}
