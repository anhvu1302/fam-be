namespace FAM.Application.Querying;

/// <summary>
/// Common request for queries with filter, sort, paging, field selection, and includes
/// </summary>
public sealed record QueryRequest
{
    /// <summary>
    /// Filter DSL string (e.g.: "name @contains 'printer' and price >= 100")
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    /// Sort string (e.g.: "-createdAt,name" means sort by createdAt desc, then by name asc)
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Page size (number of items per page)
    /// </summary>
    public int PageSize { get; init; } = 100;

    /// <summary>
    /// Fields to select (projection). Null means select all.
    /// </summary>
    public string[]? Fields { get; init; }

    /// <summary>
    /// Include relationships (e.g.: "userNodeRoles,userDevices" or "departments.manager").
    /// Comma-separated list. Supports nested includes with dot notation.
    /// </summary>
    public string? Include { get; init; }
}