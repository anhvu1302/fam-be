using FAM.Application.Querying;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.Common;

/// <summary>
/// Standard pagination query parameters for list endpoints
/// Supports filtering, sorting, pagination, field projection, and includes
/// Follows auth-service pattern for consistent API interface
/// </summary>
public sealed record PaginationQueryParameters
{
    /// <summary>
    /// Filter expression using DSL syntax
    /// Example: "isActive == true and name @contains 'test'"
    /// </summary>
    [FromQuery(Name = "filter")]
    [SwaggerParameter(
        "Filter expression using DSL syntax. Examples: \"resource @contains 'users'\", \"action == 'read' or action == 'write'\"")]
    public string? Filter { get; init; }

    /// <summary>
    /// Sort expression (comma-separated)
    /// Example: "name,-createdAt" (ascending by name, descending by createdAt)
    /// </summary>
    [FromQuery(Name = "sort")]
    [SwaggerParameter(
        "Sort expression. Use '-' prefix for descending. Examples: \"resource\", \"-createdAt\", \"resource,-action\"")]
    public string? Sort { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [FromQuery(Name = "page")]
    [SwaggerParameter("Page number (1-based)")]
    public int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    [FromQuery(Name = "pageSize")]
    [SwaggerParameter("Number of items per page (max configured in settings)")]
    public int PageSize { get; init; }

    /// <summary>
    /// Fields to include in response (comma-separated)
    /// Example: "id,resource,action"
    /// </summary>
    [FromQuery(Name = "fields")]
    [SwaggerParameter("Comma-separated fields to include. Example: \"id,resource,action,description\"")]
    public string? Fields { get; init; }

    /// <summary>
    /// Related entities to include (comma-separated)
    /// Example: "createdBy,updatedBy"
    /// </summary>
    [FromQuery(Name = "include")]
    [SwaggerParameter("Comma-separated related entities. Example: \"createdBy,updatedBy\"")]
    public string? Include { get; init; }

    /// <summary>
    /// Convert fields string to array
    /// </summary>
    public string[] GetFieldsArray()
    {
        return Fields?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    }
}

/// <summary>
///     Extension methods for building QueryRequest from query parameters
/// </summary>
public static class QueryRequestExtensions
{
    /// <summary>
    ///     Convert PaginationQueryParameters to QueryRequest
    /// </summary>
    public static QueryRequest ToQueryRequest(this PaginationQueryParameters parameters)
    {
        return new QueryRequest
        {
            Filter = parameters.Filter ?? string.Empty,
            Sort = parameters.Sort ?? string.Empty,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            Fields = parameters.GetFieldsArray(),
            Include = parameters.Include ?? string.Empty
        };
    }

    /// <summary>
    ///     Create QueryRequest from standard pagination query parameters
    /// </summary>
    public static QueryRequest ToQueryRequest(
        string? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 20,
        string? fields = null,
        string? include = null)
    {
        return new QueryRequest
        {
            Filter = filter ?? string.Empty,
            Sort = sort ?? string.Empty,
            Page = page,
            PageSize = pageSize,
            Fields = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            Include = include ?? string.Empty
        };
    }

    /// <summary>
    ///     Create QueryRequest from array-based fields parameter
    /// </summary>
    public static QueryRequest ToQueryRequest(
        string? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 20,
        string[]? fields = null,
        string? include = null)
    {
        return new QueryRequest
        {
            Filter = filter ?? string.Empty,
            Sort = sort ?? string.Empty,
            Page = page,
            PageSize = pageSize,
            Fields = fields ?? Array.Empty<string>(),
            Include = include ?? string.Empty
        };
    }
}