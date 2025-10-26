namespace FAM.Domain.Abstractions;

/// <summary>
/// Generic repository interface cho tất cả aggregates
/// </summary>
public interface IRepository<T> where T : Common.BaseEntity
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);

    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository with queryable support for advanced filtering
/// </summary>
public interface IQueryableRepository<T> : IRepository<T> where T : Common.BaseEntity
{
    /// <summary>
    /// Get IQueryable for advanced filtering and querying
    /// Note: This works at the persistence level, not domain level
    /// </summary>
    IQueryable<T> GetQueryable();
}
