namespace FAM.Domain.Abstractions;

/// <summary>
/// Generic repository interface cho tất cả aggregates
/// Supports both BaseEntity (long id) and BaseEntityGuid (Guid id)
/// </summary>
public interface IRepository<T, TId> where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);

    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for entities with long Id (backward compatibility)
/// </summary>
public interface IRepository<T> : IRepository<T, long> where T : Common.BaseEntity
{
}

/// <summary>
/// Repository with queryable support for advanced filtering
/// </summary>
public interface IQueryableRepository<T, TId> : IRepository<T, TId> where T : class
{
    /// <summary>
    /// Get IQueryable for advanced filtering and querying
    /// Note: This works at the persistence level, not domain level
    /// </summary>
    IQueryable<T> GetQueryable();
}

/// <summary>
/// Queryable repository for entities with long Id (backward compatibility)
/// </summary>
public interface IQueryableRepository<T> : IQueryableRepository<T, long>, IRepository<T> where T : Common.BaseEntity
{
}
