using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using MediatR;

namespace FAM.Application.Users.Queries;

/// <summary>
/// Query to get paginated list of users with filtering and sorting
/// </summary>
public class GetUsersQuery : IRequest<PageResult<UserDto>>
{
    public string? Filter { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string[]? Fields { get; set; }

    /// <summary>
    /// Convert to QueryRequest
    /// </summary>
    public QueryRequest ToQueryRequest() => new()
    {
        Filter = Filter,
        Sort = Sort,
        Page = Page,
        PageSize = PageSize,
        Fields = Fields
    };
}

/// <summary>
/// Paginated result for users (legacy - will be replaced by PageResult)
/// </summary>
public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalDocs { get; set; }
    public int Limit { get; set; }
    public bool HasPrevPage { get; set; }
    public bool HasNextPage { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int Offset { get; set; }
    public int? PrevPage { get; set; }
    public int? NextPage { get; set; }
    public int PagingCounter { get; set; }
    public object? Meta { get; set; }
}