namespace FAM.WebApi.Contracts.Common;

/// <summary>
/// Standard API response wrapper for successful responses
/// </summary>
public sealed record ApiSuccessResponse<TResult>(
    bool Success,
    string? Message,
    TResult? Result
)
{
    public static ApiSuccessResponse<TResult> Ok(TResult? result, string? message = null)
        => new(true, message, result);

    public static ApiSuccessResponse<TResult> Ok(TResult? result, string? message, params object?[] args)
        => new(true, message != null ? string.Format(message, args) : null, result);
}

/// <summary>
/// Standard API response wrapper for error responses
/// </summary>
public sealed record ApiErrorResponse(
    bool Success,
    List<ApiError> Errors
)
{
    public static ApiErrorResponse BadRequest(string message, string code = "BAD_REQUEST")
        => new(false, new List<ApiError> { new(message, code) });

    public static ApiErrorResponse BadRequest(params ApiError[] errors)
        => new(false, errors.ToList());

    public static ApiErrorResponse Unauthorized(string message, string code = "UNAUTHORIZED")
        => new(false, new List<ApiError> { new(message, code) });

    public static ApiErrorResponse Forbidden(string message, string code = "FORBIDDEN")
        => new(false, new List<ApiError> { new(message, code) });

    public static ApiErrorResponse NotFound(string message, string code = "NOT_FOUND")
        => new(false, new List<ApiError> { new(message, code) });

    public static ApiErrorResponse InternalServerError(string message = "An internal server error occurred", string code = "INTERNAL_SERVER_ERROR")
        => new(false, new List<ApiError> { new(message, code) });

    public static ApiErrorResponse ValidationError(params ApiError[] errors)
        => new(false, errors.ToList());

    public static ApiErrorResponse From(Exception ex, string code = "INTERNAL_SERVER_ERROR")
        => new(false, new List<ApiError> { new(ex.Message, code) });
}

/// <summary>
/// Error detail with message and i18n code
/// </summary>
public sealed record ApiError(
    string Message,
    string Code
);

/// <summary>
/// Paged response wrapper
/// </summary>
public sealed record ApiPagedResponse<TItem>(
    bool Success,
    string? Message,
    List<TItem> Data,
    PaginationMeta Pagination
)
{
    public static ApiPagedResponse<TItem> Ok(List<TItem> data, int page, int pageSize, long total, string? message = null)
        => new(true, message, data, new PaginationMeta(page, pageSize, total, (long)Math.Ceiling((double)total / pageSize), total > (page * pageSize)));
}

/// <summary>
/// Pagination metadata
/// </summary>
public sealed record PaginationMeta(
    int Page,
    int PageSize,
    long Total,
    long TotalPages,
    bool HasNextPage
);
