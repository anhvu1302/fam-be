using FAM.Application.Querying;
using FAM.Application.Querying.Validation;
using FAM.Domain.Users;

namespace FAM.Application.Users;

/// <summary>
/// FieldMap for the User entity - defines fields that can be filtered/sorted and the allowed includes
/// </summary>
public static class UserFieldMap
{
    private static FieldMap<User>? _instance;
    private static IncludeOptions? _includeOptions;

    public static FieldMap<User> Instance => _instance ??= CreateFieldMap();
    
    public static IncludeOptions GetIncludeOptions() => _includeOptions ??= CreateIncludeOptions();

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

    private static IncludeOptions CreateIncludeOptions()
    {
        return new IncludeOptions
        {
            // BEST PRACTICE: Only allow to-one relationships in includes
            // For to-many relationships (collections), use dedicated nested resource endpoints:
            // - GET /api/users/{id}/devices - to get user's devices
            // - GET /api/users/{id}/roles - to get user's node roles
            // This follows RESTful API best practices and prevents performance issues
            AllowedIncludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Currently no to-one relationships at User level
                // If User had to-one relationships like User.Profile, User.Address, etc.
                // they would be listed here
                
                // TO-MANY relationships are NOT allowed here:
                // ❌ "UserDevices" - use GET /api/users/{id}/devices instead
                // ❌ "UserNodeRoles" - use GET /api/users/{id}/roles instead
            },
            MaxDepth = 1,  // Reduced from 2 since we don't allow nested includes anymore
            MaxIncludesCount = 3  // Reduced from 5
        };
    }
}
