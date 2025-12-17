using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.Common;

/// <summary>
/// Standard API response wrapper for successful responses
/// </summary>
[SwaggerSchema(Required = new[] { "success" })]
public sealed record ApiSuccessResponse<TResult>(
    bool Success,
    string? Message,
    TResult? Result
)
{
    public static ApiSuccessResponse<TResult> Ok(TResult? result, string? message = null)
    {
        return new ApiSuccessResponse<TResult>(true, message, result);
    }

    public static ApiSuccessResponse<TResult> Ok(TResult? result, string? message, params object?[] args)
    {
        return new ApiSuccessResponse<TResult>(true, message != null ? string.Format(message, args) : null, result);
    }
}

/// <summary>
/// Standard API response wrapper for error responses
/// </summary>
[SwaggerSchema(Required = new[] { "success", "errors" })]
public sealed record ApiErrorResponse(
    bool Success,
    List<ApiError> Errors
)
{
    public static ApiErrorResponse BadRequest(string message, string code = "BAD_REQUEST")
    {
        return new ApiErrorResponse(false, new List<ApiError> { new(message, code) });
    }

    public static ApiErrorResponse BadRequest(params ApiError[] errors)
    {
        return new ApiErrorResponse(false, errors.ToList());
    }

    public static ApiErrorResponse Unauthorized(string message, string code = "UNAUTHORIZED")
    {
        return new ApiErrorResponse(false, new List<ApiError> { new(message, code) });
    }

    public static ApiErrorResponse Forbidden(string message, string code = "FORBIDDEN")
    {
        return new ApiErrorResponse(false, new List<ApiError> { new(message, code) });
    }

    public static ApiErrorResponse NotFound(string message, string code = "NOT_FOUND")
    {
        return new ApiErrorResponse(false, new List<ApiError> { new(message, code) });
    }

    public static ApiErrorResponse InternalServerError(string message = "An internal server error occurred",
        string code = "INTERNAL_SERVER_ERROR")
    {
        return new ApiErrorResponse(false, new List<ApiError> { new(message, code) });
    }

    public static ApiErrorResponse ValidationError(params ApiError[] errors)
    {
        return new ApiErrorResponse(false, errors.ToList());
    }

    public static ApiErrorResponse From(Exception ex, string code = "INTERNAL_SERVER_ERROR")
    {
        return new ApiErrorResponse(false, new List<ApiError> { new(ex.Message, code) });
    }
}

/// <summary>
/// Error detail with message and i18n code
/// </summary>
[SwaggerSchema(Required = new[] { "message", "code" })]
public sealed record ApiError(
    string Message,
    string Code
);

/// <summary>
/// Paged response wrapper
/// </summary>
[SwaggerSchema(Required = new[] { "success", "data", "pagination" })]
public sealed record ApiPagedResponse<TItem>(
    bool Success,
    string? Message,
    List<TItem> Data,
    PaginationMeta Pagination
)
{
    public static ApiPagedResponse<TItem> Ok(List<TItem> data, int page, int pageSize, long total,
        string? message = null)
    {
        return new ApiPagedResponse<TItem>(true, message, data,
            new PaginationMeta(page, pageSize, total, (long)Math.Ceiling((double)total / pageSize),
                total > page * pageSize));
    }
}

/// <summary>
/// Pagination metadata
/// </summary>
[SwaggerSchema(Required = new[] { "page", "pageSize", "total", "totalPages", "hasNextPage" })]
public sealed record PaginationMeta(
    int Page,
    int PageSize,
    long Total,
    long TotalPages,
    bool HasNextPage
);
