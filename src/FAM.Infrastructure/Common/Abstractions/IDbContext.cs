using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FAM.Infrastructure.Common.Abstractions;

/// <summary>
/// Abstraction for DbContext to support multiple database providers
/// Allows easy switching between SQL Server, PostgreSQL, MySQL, etc.
/// Follows Clean Architecture by decoupling repositories from specific DbContext implementations
/// </summary>
public interface IDbContext : IDisposable
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    ChangeTracker ChangeTracker { get; }
    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
