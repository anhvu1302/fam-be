using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL repository for SystemSetting
/// </summary>
public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public SystemSettingRepository(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SystemSetting?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<SystemSetting>(entity) : null;
    }

    public async Task<IEnumerable<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.SystemSettings
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SystemSetting>(e));
    }

    public async Task<IEnumerable<SystemSetting>> FindAsync(
        Expression<Func<SystemSetting, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task AddAsync(SystemSetting entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<SystemSettingEf>(entity);
        await _context.SystemSettings.AddAsync(efEntity, cancellationToken);
    }

    public void Update(SystemSetting entity)
    {
        var efEntity = _context.SystemSettings.Local.FirstOrDefault(s => s.Id == entity.Id);
        if (efEntity != null)
        {
            _mapper.Map(entity, efEntity);
        }
        else
        {
            efEntity = _mapper.Map<SystemSettingEf>(entity);
            _context.SystemSettings.Update(efEntity);
        }
    }

    public void Delete(SystemSetting entity)
    {
        var efEntity = _context.SystemSettings.Local.FirstOrDefault(s => s.Id == entity.Id);
        if (efEntity != null)
        {
            efEntity.IsDeleted = true;
            efEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings.AnyAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<SystemSetting>(entity) : null;
    }

    public async Task<IReadOnlyList<SystemSetting>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.SystemSettings
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SystemSetting>(e)).ToList();
    }

    public async Task<IReadOnlyList<SystemSetting>> GetByGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        var entities = await _context.SystemSettings
            .Where(s => s.Group == group.ToLowerInvariant() && !s.IsDeleted)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SystemSetting>(e)).ToList();
    }

    public async Task<IReadOnlyList<SystemSetting>> GetVisibleSettingsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.SystemSettings
            .Where(s => s.IsVisible && !s.IsDeleted)
            .OrderBy(s => s.Group)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SystemSetting>(e)).ToList();
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

    public async Task<bool> KeyExistsAsync(string key, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SystemSettings.Where(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task UpdateValueAsync(string key, string? value, long? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        await _context.SystemSettings
            .Where(s => s.Key == key.ToLowerInvariant() && !s.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Value, value)
                .SetProperty(x => x.LastModifiedBy, modifiedBy)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task BulkUpdateAsync(Dictionary<string, string?> keyValues, long? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in keyValues)
        {
            await UpdateValueAsync(kvp.Key, kvp.Value, modifiedBy, cancellationToken);
        }
    }
}
