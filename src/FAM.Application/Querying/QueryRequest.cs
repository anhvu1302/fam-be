namespace FAM.Application.Querying;

/// <summary>
/// Request chung cho query với filter, sort, paging, và field selection
/// </summary>
public sealed record QueryRequest
{
    /// <summary>
    /// Filter DSL string (ví dụ: "name @contains 'printer' and price >= 100")
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    /// Sort string (ví dụ: "-createdAt,name" nghĩa là sort by createdAt desc, then by name asc)
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Page size (số items per page)
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Fields to select (projection). Null nghĩa là select all.
    /// </summary>
    public string[]? Fields { get; init; }
}
