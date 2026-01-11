using System.Linq.Expressions;

using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// Base repository for EF Core
/// Pragmatic Architecture - uses single Domain entity
/// Provider-agnostic (can work with any EF Core provider)
/// Follows Clean Architecture by depending on IDbContext abstraction
/// </summary>
public abstract class BaseRepository<TEntity> : BasePagedRepository<TEntity>
    where TEntity : class
{
    protected readonly IDbContext Context;
    protected DbSet<TEntity> DbSet => Context.Set<TEntity>();

    protected BaseRepository(IDbContext context) : base()
    {
        Context = context;
    }

    /// <summary>
    /// Apply dynamic sorting to EF Core query
    /// </summary>
    protected IQueryable<TEntity> ApplySort(
        IQueryable<TEntity> query,
        string? sort,
        Func<string, Expression<Func<TEntity, object>>> getFieldExpression,
        Expression<Func<TEntity, object>> defaultSort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(defaultSort);
        }

        string[] sortParts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<TEntity>? orderedQuery = null;

        foreach (string sortPart in sortParts)
        {
            string trimmed = sortPart.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            bool descending = trimmed.StartsWith('-');
            string fieldName = descending ? trimmed[1..] : trimmed;

            try
            {
                Expression<Func<TEntity, object>> expression = getFieldExpression(fieldName.ToLowerInvariant());

                if (orderedQuery == null)
                {
                    orderedQuery = descending
                        ? query.OrderByDescending(expression)
                        : query.OrderBy(expression);
                }
                else
                {
                    orderedQuery = descending
                        ? orderedQuery.ThenByDescending(expression)
                        : orderedQuery.ThenBy(expression);
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting");
            }
        }

        return orderedQuery ?? query.OrderBy(defaultSort);
    }
}
