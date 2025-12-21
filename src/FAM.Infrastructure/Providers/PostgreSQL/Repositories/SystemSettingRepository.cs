using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL repository for SystemSetting
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly PostgreSqlDbContext _context;

    public SystemSettingRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
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
        return await _context.SystemSettings.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SystemSetting entity, CancellationToken cancellationToken = default)
    {
        await _context.SystemSettings.AddAsync(entity, cancellationToken);
    }

    public void Update(SystemSetting entity)
    {
        var trackedEntry = _context.ChangeTracker.Entries<SystemSetting>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _context.SystemSettings.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(SystemSetting entity)
    {
        var trackedEntity = _context.SystemSettings.Local.FirstOrDefault(s => s.Id == entity.Id);
        if (trackedEntity != null)
        {
            trackedEntity.IsDeleted = true;
            trackedEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings.AnyAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetByGroupAsync(string group,
        CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .Where(s => s.Group == group.ToLowerInvariant() && !s.IsDeleted)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemSetting>> GetVisibleSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .Where(s => s.IsVisible && !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
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
            _context.SystemSettings.Where(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task UpdateValueAsync(string key, string? value, long? modifiedBy = null,
        CancellationToken cancellationToken = default)
    {
        await _context.SystemSettings
            .Where(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Value, value)
                    .SetProperty(x => x.LastModifiedBy, modifiedBy)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task BulkUpdateAsync(Dictionary<string, string?> keyValues, long? modifiedBy = null,
        CancellationToken cancellationToken = default)
    {
        foreach (KeyValuePair<string, string?> kvp in keyValues)
            await UpdateValueAsync(kvp.Key, kvp.Value, modifiedBy, cancellationToken);
    }
}
