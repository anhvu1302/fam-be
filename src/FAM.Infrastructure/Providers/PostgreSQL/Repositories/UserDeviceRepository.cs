using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserDeviceRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class UserDeviceRepository : IUserDeviceRepository
{
    private readonly PostgreSqlDbContext _context;

    public UserDeviceRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<UserDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<UserDevice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserDevice>> FindAsync(Expression<Func<UserDevice, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserDevice entity, CancellationToken cancellationToken = default)
    {
        await _context.UserDevices.AddAsync(entity, cancellationToken);
    }

    public void Update(UserDevice entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        EntityEntry<UserDevice>? trackedEntry = _context.ChangeTracker.Entries<UserDevice>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _context.UserDevices.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(UserDevice entity)
    {
        _context.UserDevices.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);
    }

    public async Task<IEnumerable<UserDevice>> GetByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserDevice>> GetActiveDevicesByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsDeviceIdTakenAsync(string deviceId, Guid? excludeDeviceId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserDevice> query = _context.UserDevices.Where(d => d.DeviceId == deviceId);
        if (excludeDeviceId.HasValue) query = query.Where(d => d.Id != excludeDeviceId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task DeactivateDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        UserDevice? entity = await _context.UserDevices.FindAsync(new object[] { deviceId }, cancellationToken);
        if (entity != null)
        {
            entity.Deactivate();
            _context.UserDevices.Update(entity);
        }
    }

    public async Task DeactivateAllUserDevicesAsync(long userId, string? excludeDeviceId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserDevice> query = _context.UserDevices
            .Where(d => d.UserId == userId && !d.IsDeleted);

        // Exclude device by DeviceId (fingerprint string), not by Guid ID
        if (!string.IsNullOrEmpty(excludeDeviceId)) query = query.Where(d => d.DeviceId != excludeDeviceId);

        List<UserDevice> entities = await query.ToListAsync(cancellationToken);

        foreach (UserDevice entity in entities) entity.Deactivate();

        _context.UserDevices.UpdateRange(entities);
    }

    public async Task<UserDevice?> FindByRefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken && !d.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Get IQueryable for advanced filtering
    /// </summary>
    public IQueryable<UserDevice> GetQueryable()
    {
        return _context.UserDevices.AsNoTracking();
    }

    /// <summary>
    /// Update last activity time for a device
    /// </summary>
    public async Task<bool> UpdateLastActivityAsync(string deviceId, string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        UserDevice? entity = await _context.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.IsActive && !d.IsDeleted, cancellationToken);

        if (entity == null)
            return false;

        entity.UpdateActivity(ipAddress);
        entity.UpdatedAt = DateTime.UtcNow;

        return true;
    }
}
