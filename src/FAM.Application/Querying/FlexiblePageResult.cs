namespace FAM.Application.Querying;

/// <summary>
/// Page result with flexible item type (can be DTO or Dictionary for field selection)
/// </summary>
public sealed record FlexiblePageResult
{
    public object Items { get; init; } = Array.Empty<object>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public FlexiblePageResult(object items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
