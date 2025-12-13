using System.Linq.Expressions;

using AutoMapper;

using FAM.Infrastructure.Providers.PostgreSQL;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// Base repository for PostgreSQL with EF Core specific implementations
/// Inherits common helpers from BasePagedRepository
/// </summary>
public abstract class BasePostgreSqlRepository<TDomain, TEf> : BasePagedRepository<TDomain, TEf>
    where TDomain : class
    where TEf : class
{
    protected readonly PostgreSqlDbContext Context;

    protected BasePostgreSqlRepository(PostgreSqlDbContext context, IMapper mapper) : base(mapper)
    {
        Context = context;
    }

    /// <summary>
    /// Apply dynamic sorting to EF Core query
    /// </summary>
    protected IQueryable<TEf> ApplySort(
        IQueryable<TEf> query,
        string? sort,
        Func<string, Expression<Func<TEf, object>>> getFieldExpression,
        Expression<Func<TEf, object>> defaultSort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(defaultSort);

        var sortParts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<TEf>? orderedQuery = null;

        foreach (var sortPart in sortParts)
        {
            var trimmed = sortPart.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var descending = trimmed.StartsWith('-');
            var fieldName = descending ? trimmed[1..] : trimmed;

            try
            {
                Expression<Func<TEf, object>> expression = getFieldExpression(fieldName.ToLowerInvariant());

                if (orderedQuery == null)
                    orderedQuery = descending
                        ? query.OrderByDescending(expression)
                        : query.OrderBy(expression);
                else
                    orderedQuery = descending
                        ? orderedQuery.ThenByDescending(expression)
                        : orderedQuery.ThenBy(expression);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting");
            }
        }

        return orderedQuery ?? query.OrderBy(defaultSort);
    }
}
