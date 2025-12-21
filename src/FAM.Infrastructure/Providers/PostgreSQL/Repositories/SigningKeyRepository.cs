using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL repository for SigningKey
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class SigningKeyRepository : ISigningKeyRepository
{
    private readonly PostgreSqlDbContext _context;

    public SigningKeyRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<SigningKey?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .Where(k => !k.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SigningKey>> FindAsync(
        Expression<Func<SigningKey, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await _context.SigningKeys.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SigningKey entity, CancellationToken cancellationToken = default)
    {
        await _context.SigningKeys.AddAsync(entity, cancellationToken);
    }

    public void Update(SigningKey entity)
    {
        var trackedEntry = _context.ChangeTracker.Entries<SigningKey>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _context.SigningKeys.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(SigningKey entity)
    {
        var trackedEntity = _context.SigningKeys.Local.FirstOrDefault(k => k.Id == entity.Id);
        if (trackedEntity != null)
        {
            trackedEntity.IsDeleted = true;
            trackedEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .AnyAsync(k => k.Id == id && !k.IsDeleted, cancellationToken);
    }

    public async Task<SigningKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .FirstOrDefaultAsync(k => k.KeyId == keyId && !k.IsDeleted, cancellationToken);
    }

    public async Task<SigningKey?> GetActiveKeyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .Where(k => k.IsActive && !k.IsRevoked && !k.IsDeleted)
            .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SigningKey>> GetVerificationKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .Where(k => !k.IsRevoked && !k.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SigningKey>> GetAllActiveKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .Where(k => !k.IsRevoked && !k.IsDeleted)
            .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(k => k.IsActive)
            .ThenByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeactivateAllExceptAsync(long keyId, CancellationToken cancellationToken = default)
    {
        await _context.SigningKeys
            .Where(k => k.Id != keyId && k.IsActive && !k.IsDeleted)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(k => k.IsActive, false)
                    .SetProperty(k => k.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task<IReadOnlyList<SigningKey>> GetExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        return await _context.SigningKeys
            .Where(k => !k.IsDeleted && k.ExpiresAt != null && k.ExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SigningKey>> GetKeysExpiringWithinAsync(TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        DateTime expiryThreshold = now.Add(timeSpan);
        return await _context.SigningKeys
            .Where(k => !k.IsDeleted && !k.IsRevoked && k.ExpiresAt != null)
            .Where(k => k.ExpiresAt > now && k.ExpiresAt <= expiryThreshold)
            .ToListAsync(cancellationToken);
    }
}
