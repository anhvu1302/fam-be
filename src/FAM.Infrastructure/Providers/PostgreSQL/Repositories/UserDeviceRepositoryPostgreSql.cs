using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserDeviceRepository
/// </summary>
public class UserDeviceRepositoryPostgreSql : IUserDeviceRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public UserDeviceRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserDevices.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<UserDevice>(entity) : null;
    }

    public async Task<IEnumerable<UserDevice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserDevices.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDevice>>(entities);
    }

    public async Task<IEnumerable<UserDevice>> FindAsync(Expression<Func<UserDevice, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.UserDevices.ToListAsync(cancellationToken);
        var allUserDevices = _mapper.Map<IEnumerable<UserDevice>>(allEntities);
        return allUserDevices.Where(predicate.Compile());
    }

    public async Task AddAsync(UserDevice entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<UserDeviceEf>(entity);
        await _context.UserDevices.AddAsync(efEntity, cancellationToken);
    }

    public void Update(UserDevice entity)
    {
        var efEntity = _mapper.Map<UserDeviceEf>(entity);
        efEntity.UpdatedAt = DateTime.UtcNow;

        var trackedEntry = _context.ChangeTracker.Entries<UserDeviceEf>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(efEntity);
        }
        else
        {
            _context.UserDevices.Attach(efEntity);
            _context.Entry(efEntity).State = EntityState.Modified;
        }
    }

    public void Delete(UserDevice entity)
    {
        var efEntity = _mapper.Map<UserDeviceEf>(entity);

        // Check if entity is already tracked
        var trackedEntry = _context.UserDevices.Local.FirstOrDefault(d => d.Id == efEntity.Id);
        if (trackedEntry != null)
        {
            // Use the already-tracked entity
            _context.UserDevices.Remove(trackedEntry);
        }
        else
        {
            // Entity not tracked, remove the mapped entity
            _context.UserDevices.Remove(efEntity);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);
        return entity != null ? _mapper.Map<UserDevice>(entity) : null;
    }

    public async Task<IEnumerable<UserDevice>> GetByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDevice>>(entities);
    }

    public async Task<IEnumerable<UserDevice>> GetActiveDevicesByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDevice>>(entities);
    }

    public async Task<bool> IsDeviceIdTakenAsync(string deviceId, Guid? excludeDeviceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserDevices.Where(d => d.DeviceId == deviceId);
        if (excludeDeviceId.HasValue) query = query.Where(d => d.Id != excludeDeviceId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task DeactivateDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserDevices.FindAsync(new object[] { deviceId }, cancellationToken);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.RefreshToken = null;
            entity.RefreshTokenExpiresAt = null;
            _context.UserDevices.Update(entity);
        }
    }

    public async Task DeactivateAllUserDevicesAsync(long userId, string? excludeDeviceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserDevices
            .Where(d => d.UserId == userId && !d.IsDeleted);

        // Exclude device by DeviceId (fingerprint string), not by Guid ID
        if (!string.IsNullOrEmpty(excludeDeviceId)) query = query.Where(d => d.DeviceId != excludeDeviceId);

        var entities = await query.ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.IsActive = false;
            entity.RefreshToken = null;
            entity.RefreshTokenExpiresAt = null;
        }

        _context.UserDevices.UpdateRange(entities);
    }

    public async Task<UserDevice?> FindByRefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserDevices
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken && !d.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<UserDevice>(entity) : null;
    }

    /// <summary>
    /// Get IQueryable for EF entities (not domain) - for advanced filtering
    /// <summary>
    /// Get IQueryable for EF entities (not domain) - for advanced filtering
    /// Note: Returns EF entities which will be mapped to domain later
    /// </summary>
    public IQueryable<UserDevice> GetQueryable()
    {
        // We need to return domain UserDevice but work with EF under the hood
        // For simplicity, we'll use a workaround: query EF then map in-memory
        // A better approach would be to use projection expressions
        throw new NotImplementedException("Use GetQueryableEf() instead for proper EF Core querying");
    }

    /// <summary>
    /// Get IQueryable of EF entities for filtering
    /// </summary>
    public IQueryable<UserDeviceEf> GetQueryableEf()
    {
        return _context.UserDevices.AsNoTracking();
    }

    /// <summary>
    /// Map EF entities to domain after querying
    /// </summary>
    public IEnumerable<UserDevice> MapToDomain(IEnumerable<UserDeviceEf> efEntities)
    {
        return _mapper.Map<IEnumerable<UserDevice>>(efEntities);
    }
}