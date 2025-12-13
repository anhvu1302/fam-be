using FAM.Application.Querying.Validation;
using FAM.Infrastructure.PersistenceModels.Ef;

namespace FAM.Infrastructure.Providers.PostgreSQL.Querying;

/// <summary>
/// FieldMap cho UserEf (EF Core entity) - dùng cho filter DSL
/// </summary>
public static class UserEfFieldMap
{
    private static FieldMap<UserEf>? _instance;

    public static FieldMap<UserEf> Instance => _instance ??= CreateFieldMap();

    private static FieldMap<UserEf> CreateFieldMap()
    {
        return new FieldMap<UserEf>()
            .Add("id", x => x.Id, true, true)
            .Add("username", x => x.Username, true, true)
            .Add("email", x => x.Email, true, true)
            .Add("fullname", x => x.FullName, true, true)
            .Add("createdAt", x => x.CreatedAt, true, true)
            .Add("updatedAt", x => x.UpdatedAt, true, true)
            .Add("isDeleted", x => x.IsDeleted, true, true);
    }
}
