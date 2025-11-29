using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL repository for SigningKey
/// </summary>
public class SigningKeyRepository : ISigningKeyRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public SigningKeyRepository(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SigningKey?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SigningKeys
            .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<SigningKey>(entity) : null;
    }

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.SigningKeys
            .Where(k => !k.IsDeleted)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SigningKey>(e));
    }

    public async Task<IEnumerable<SigningKey>> FindAsync(
        Expression<Func<SigningKey, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // For simple queries, get all and filter in memory
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task AddAsync(SigningKey entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<SigningKeyEf>(entity);
        await _context.SigningKeys.AddAsync(efEntity, cancellationToken);
    }

    public void Update(SigningKey entity)
    {
        var efEntity = _context.SigningKeys.Local.FirstOrDefault(k => k.Id == entity.Id);
        if (efEntity != null)
        {
            _mapper.Map(entity, efEntity);
        }
        else
        {
            efEntity = _mapper.Map<SigningKeyEf>(entity);
            _context.SigningKeys.Update(efEntity);
        }
    }

    public void Delete(SigningKey entity)
    {
        var efEntity = _context.SigningKeys.Local.FirstOrDefault(k => k.Id == entity.Id);
        if (efEntity != null)
        {
            efEntity.IsDeleted = true;
            efEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SigningKeys
            .AnyAsync(k => k.Id == id && !k.IsDeleted, cancellationToken);
    }

    public async Task<SigningKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SigningKeys
            .FirstOrDefaultAsync(k => k.KeyId == keyId && !k.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<SigningKey>(entity) : null;
    }

    public async Task<SigningKey?> GetActiveKeyAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _context.SigningKeys
            .Where(k => k.IsActive && !k.IsRevoked && !k.IsDeleted)
            .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return entity != null ? _mapper.Map<SigningKey>(entity) : null;
    }

    public async Task<IReadOnlyList<SigningKey>> GetVerificationKeysAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.SigningKeys
            .Where(k => !k.IsRevoked && !k.IsDeleted)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SigningKey>(e)).ToList();
    }

    public async Task<IReadOnlyList<SigningKey>> GetAllActiveKeysAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.SigningKeys
            .Where(k => !k.IsRevoked && !k.IsDeleted)
            .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(k => k.IsActive)
            .ThenByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SigningKey>(e)).ToList();
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
        var now = DateTime.UtcNow;
        var entities = await _context.SigningKeys
            .Where(k => !k.IsDeleted && k.ExpiresAt != null && k.ExpiresAt < now)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SigningKey>(e)).ToList();
    }

    public async Task<IReadOnlyList<SigningKey>> GetKeysExpiringWithinAsync(TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiryThreshold = now.Add(timeSpan);
        var entities = await _context.SigningKeys
            .Where(k => !k.IsDeleted && !k.IsRevoked && k.ExpiresAt != null)
            .Where(k => k.ExpiresAt > now && k.ExpiresAt <= expiryThreshold)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<SigningKey>(e)).ToList();
    }
}