using FAM.Application.Querying.Validation;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.Providers.MongoDB.Querying;

/// <summary>
/// FieldMap for MongoDB User documents
/// Maps field names to document properties for filtering/sorting
/// </summary>
public sealed class MongoUserFieldMap
{
    private static readonly Lazy<FieldMap<UserMongo>> _instance = new(() =>
    {
        var map = new FieldMap<UserMongo>();

        // Map fields to MongoDB document properties
        map.Add("id", u => u.Id, canFilter: true, canSort: true, canSelect: true);
        map.Add("username", u => u.Username, canFilter: true, canSort: true, canSelect: true);
        map.Add("email", u => u.Email, canFilter: true, canSort: true, canSelect: true);
        map.Add("fullname", u => u.FullName ?? string.Empty, canFilter: true, canSort: true, canSelect: true);
        map.Add("createdAt", u => u.CreatedAt, canFilter: true, canSort: true, canSelect: true);
        map.Add("updatedAt", u => u.UpdatedAt ?? DateTime.MinValue, canFilter: true, canSort: true, canSelect: true);
        map.Add("isDeleted", u => u.IsDeleted, canFilter: true, canSort: true, canSelect: true);

        return map;
    });

    public static FieldMap<UserMongo> Instance => _instance.Value;

    private MongoUserFieldMap() { }
}
