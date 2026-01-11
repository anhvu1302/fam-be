using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL repository for SystemSetting
/// Uses Pragmatic Architecture - directly works with Domain entities
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly IDbContext _context;
    protected DbSet<SystemSetting> DbSet => _context.Set<SystemSetting>();

    public SystemSettingRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SystemSetting>> FindAsync(
        Expression<Func<SystemSetting, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SystemSetting entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(SystemSetting entity)
    {
        EntityEntry<SystemSetting>? trackedEntry = _context.ChangeTracker.Entries<SystemSetting>()
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

    public void Delete(SystemSetting entity)
    {
        SystemSetting? trackedEntity = DbSet.Local.FirstOrDefault(s => s.Id == entity.Id);
        if (trackedEntity != null)
        {
            trackedEntity.IsDeleted = true;
            trackedEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetByGroupAsync(string group,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.Group == group.ToLowerInvariant() && !s.IsDeleted)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetVisibleSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsVisible && !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => !s.IsDeleted)
            .Select(s => s.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(string key, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SystemSetting> query =
            DbSet.Where(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task UpdateValueAsync(string key, string? value, long? updatedById = null,
        CancellationToken cancellationToken = default)
    {
        await DbSet
            .Where(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Value, value)
                    .SetProperty(x => x.UpdatedById, updatedById)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task BulkUpdateAsync(Dictionary<string, string?> keyValues, long? updatedById = null,
        CancellationToken cancellationToken = default)
    {
        foreach (KeyValuePair<string, string?> kvp in keyValues)
        {
            await UpdateValueAsync(kvp.Key, kvp.Value, updatedById, cancellationToken);
        }
    }
}
