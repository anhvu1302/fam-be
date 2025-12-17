using System.Linq.Expressions;

using FAM.Domain.Common.Base;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Base repository interface với common read operations
/// Dùng cho tất cả entities (có hoặc không có Id)
/// </summary>
public interface IReadRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get IQueryable for advanced filtering and querying
    /// Note: This works at the persistence level, not domain level
    /// </summary>
    IQueryable<T> GetQueryable()
    {
        throw new NotImplementedException(
            "GetQueryable not implemented for this repository. Override in derived class if needed.");
    }
}

/// <summary>
/// Repository for entities with Id (any type)
/// Supports GetByIdAsync, ExistsAsync, Update operations
/// </summary>
public interface IRepository<T, TId> : IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities with filtering, sorting and pagination applied at database level.
    /// Follows auth-service pattern for efficient querying.
    /// </summary>
    Task<(IEnumerable<T> Items, long Total)> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        string? sort,
        int page,
        int pageSize,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "GetPagedAsync not implemented for this repository. Override in derived class if needed.");
    }
}

/// <summary>
/// Repository for entities without Id (junction/linking tables)
/// Only supports GetAll, Find, Add, Delete operations
/// </summary>
public interface IJunctionRepository<T> : IReadRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Delete(T entity);
}

/// <summary>
/// Repository for entities with long Id (backward compatibility)
/// </summary>
public interface IRepository<T> : IRepository<T, long> where T : BaseEntity
{
}
