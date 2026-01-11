using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Common.Helpers;

/// <summary>
/// Helper methods for building efficient EF Core queries
/// Follows auth-service pattern for repository operations
/// </summary>
public static class QueryHelper
{
    /// <summary>
    /// Apply pagination to IQueryable
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        int skip = (page - 1) * pageSize;
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
        {
            return query;
        }

        foreach (Expression<Func<T, object>> include in includes)
        {
            query = query.Include(include);
        }

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
        long total = await query.LongCountAsync(cancellationToken);

        // Apply pagination and get items
        List<T> items = await query
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
