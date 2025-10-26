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
            .Add("id", x => x.Id, canFilter: true, canSort: true)
            .Add("username", x => x.Username, canFilter: true, canSort: true)
            .Add("email", x => x.Email, canFilter: true, canSort: true)
            .Add("fullname", x => x.FullName, canFilter: true, canSort: true)
            .Add("createdAt", x => x.CreatedAt, canFilter: true, canSort: true)
            .Add("updatedAt", x => x.UpdatedAt, canFilter: true, canSort: true)
            .Add("isDeleted", x => x.IsDeleted, canFilter: true, canSort: true);
    }
}
