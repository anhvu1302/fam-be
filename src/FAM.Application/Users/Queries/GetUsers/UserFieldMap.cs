using System.Linq.Expressions;
using FAM.Application.Querying;
using FAM.Application.Querying.Validation;
using FAM.Domain.Users;

namespace FAM.Application.Users.Queries.GetUsers;

/// <summary>
/// Field map for User domain entity - defines allowed fields for filtering, sorting, and includes
/// </summary>
public class UserFieldMap : BaseFieldMap<User>
{
    private static readonly Lazy<UserFieldMap> _instance = new(() => new UserFieldMap());

    private UserFieldMap()
    {
    }

    public static UserFieldMap Instance => _instance.Value;

    public override FieldMap<User> Fields { get; } = new FieldMap<User>()
        .Add("id", u => u.Id)
        .Add("username", u => u.Username.Value)
        .Add("email", u => u.Email.Value)
        .Add("fullName", u => u.FullName!)
        .Add("firstName", u => u.FirstName!)
        .Add("lastName", u => u.LastName!)
        .Add("isActive", u => u.IsActive)
        .Add("isEmailVerified", u => u.IsEmailVerified)
        .Add("isPhoneVerified", u => u.IsPhoneVerified)
        .Add("twoFactorEnabled", u => u.TwoFactorEnabled)
        .Add("createdAt", u => u.CreatedAt)
        .Add("updatedAt", u => u.UpdatedAt!)
        .Add("deletedAt", u => u.DeletedAt!)
        .Add("lastLoginAt", u => u.LastLoginAt!);

    protected override Dictionary<string, Expression<Func<User, object>>> AllowedIncludes { get; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["devices"] = u => u.UserDevices,
            ["userdevices"] = u => u.UserDevices,
            ["noderoles"] = u => u.UserNodeRoles,
            ["usernoderoles"] = u => u.UserNodeRoles
        };
}