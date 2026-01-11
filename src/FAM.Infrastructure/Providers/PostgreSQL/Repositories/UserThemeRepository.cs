using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserThemeRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class UserThemeRepository : IUserThemeRepository
{
    private readonly IDbContext _context;
    protected DbSet<UserTheme> DbSet => _context.Set<UserTheme>();

    public UserThemeRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<UserTheme?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<UserTheme?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(ut => ut.UserId == userId && !ut.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(ut => ut.UserId == userId && !ut.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<UserTheme>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserTheme>> FindAsync(Expression<Func<UserTheme, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserTheme entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(UserTheme entity)
    {
        EntityEntry<UserTheme>? trackedEntry = _context.ChangeTracker.Entries<UserTheme>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            DbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(UserTheme entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(ut => ut.Id == id && !ut.IsDeleted, cancellationToken);
    }
}
