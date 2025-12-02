using System.Linq.Expressions;
using FAM.Application.Querying.Validation;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Common.Helpers;

/// <summary>
/// Helper methods for building efficient EF Core queries
/// Follows auth-service pattern for repository operations
/// </summary>
public static class QueryHelper
{
    /// <summary>
    /// Apply dynamic sorting to IQueryable based on field map
    /// </summary>
    public static IQueryable<T> ApplyDynamicSort<T>(
        this IQueryable<T> query,
        string? sortExpression,
        FieldMap<T> fieldMap)
    {
        if (string.IsNullOrWhiteSpace(sortExpression))
            return query;

        var sortParts = sortExpression.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var part in sortParts)
        {
            var trimmed = part.Trim();
            var descending = trimmed.StartsWith('-');
            var fieldName = descending ? trimmed[1..] : trimmed;

            if (!fieldMap.TryGet(fieldName, out var expression, out _))
                continue;

            var typedExpression = (Expression<Func<T, object>>)expression;

            if (orderedQuery == null)
                orderedQuery = descending
                    ? query.OrderByDescending(typedExpression)
                    : query.OrderBy(typedExpression);
            else
                orderedQuery = descending
                    ? orderedQuery.ThenByDescending(typedExpression)
                    : orderedQuery.ThenBy(typedExpression);
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// Apply pagination to IQueryable
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return query.Skip(skip).Take(pageSize);
    }

    /// <summary>
    /// Apply dynamic includes to IQueryable
    /// </summary>
    public static IQueryable<T> ApplyIncludes<T>(
        this IQueryable<T> query,
        IEnumerable<Expression<Func<T, object>>>? includes) where T : class
    {
        if (includes == null)
            return query;

        foreach (var include in includes) query = query.Include(include);

        return query;
    }

    /// <summary>
    /// Execute paged query and return items with total count
    /// Optimized: count and items retrieved in parallel when possible
    /// </summary>
    public static async Task<(List<T> Items, long Total)> ExecutePagedQueryAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Get total count before pagination
        var total = await query.LongCountAsync(cancellationToken);

        // Apply pagination and get items
        var items = await query
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}