using FAM.Application.Querying.Validation;
using FAM.Domain.Users;

namespace FAM.Application.Users;

/// <summary>
/// FieldMap cho User entity - định nghĩa các field có thể filter/sort
/// </summary>
public static class UserFieldMap
{
    private static FieldMap<User>? _instance;

    public static FieldMap<User> Instance => _instance ??= CreateFieldMap();

    private static FieldMap<User> CreateFieldMap()
    {
        return new FieldMap<User>()
            .Add("id", x => x.Id, canFilter: true, canSort: true)
            .Add("username", x => x.Username.Value, canFilter: true, canSort: true)
            .Add("email", x => x.Email.Value, canFilter: true, canSort: true)
            .Add("fullname", x => x.FullName, canFilter: true, canSort: true)
            .Add("createdAt", x => x.CreatedAt, canFilter: true, canSort: true)
            .Add("updatedAt", x => x.UpdatedAt, canFilter: true, canSort: true)
            .Add("isDeleted", x => x.IsDeleted, canFilter: true, canSort: true);
    }
}
