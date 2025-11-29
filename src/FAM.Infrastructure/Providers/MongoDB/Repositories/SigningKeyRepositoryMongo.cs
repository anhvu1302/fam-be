using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Providers.MongoDB;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of ISigningKeyRepository
/// </summary>
public class SigningKeyRepositoryMongo : ISigningKeyRepository
{
    private readonly MongoDbContext _context;
    private readonly IMongoCollection<SigningKeyMongo> _collection;
    private readonly IMapper _mapper;
    private static long _idCounter = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public SigningKeyRepositoryMongo(MongoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _collection = _context.GetCollection<SigningKeyMongo>("signingKeys");
    }

    public async Task<SigningKey?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(k => k.DomainId == id && !k.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<SigningKey>(document) : null;
    }

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(k => !k.IsDeleted)
            .ToListAsync(cancellationToken);
        return documents.Select(d => _mapper.Map<SigningKey>(d));
    }

    public async Task<IEnumerable<SigningKey>> FindAsync(
        Expression<Func<SigningKey, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task AddAsync(SigningKey entity, CancellationToken cancellationToken = default)
    {
        var document = _mapper.Map<SigningKeyMongo>(entity);
        document.DomainId = Interlocked.Increment(ref _idCounter);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);

        // Set the domain entity ID using reflection
        var idProperty = typeof(SigningKey).GetProperty("Id");
        idProperty?.SetValue(entity, document.DomainId);
    }

    public void Update(SigningKey entity)
    {
        var document = _mapper.Map<SigningKeyMongo>(entity);
        var filter = Builders<SigningKeyMongo>.Filter.Eq(k => k.DomainId, entity.Id);
        _collection.ReplaceOne(filter, document);
    }

    public void Delete(SigningKey entity)
    {
        var filter = Builders<SigningKeyMongo>.Filter.Eq(k => k.DomainId, entity.Id);
        var update = Builders<SigningKeyMongo>.Update
            .Set(k => k.IsDeleted, true)
            .Set(k => k.DeletedAt, DateTime.UtcNow);
        _collection.UpdateOne(filter, update);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(k => k.DomainId == id && !k.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    public async Task<SigningKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(k => k.KeyId == keyId && !k.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<SigningKey>(document) : null;
    }

    public async Task<SigningKey?> GetActiveKeyAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var document = await _collection
            .Find(k => k.IsActive && !k.IsRevoked && !k.IsDeleted &&
                       (k.ExpiresAt == null || k.ExpiresAt > now))
            .SortByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<SigningKey>(document) : null;
    }

    public async Task<IReadOnlyList<SigningKey>> GetVerificationKeysAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(k => !k.IsRevoked && !k.IsDeleted)
            .ToListAsync(cancellationToken);
        return documents.Select(d => _mapper.Map<SigningKey>(d)).ToList();
    }

    public async Task<IReadOnlyList<SigningKey>> GetAllActiveKeysAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var documents = await _collection
            .Find(k => !k.IsRevoked && !k.IsDeleted && (k.ExpiresAt == null || k.ExpiresAt > now))
            .SortByDescending(k => k.IsActive)
            .ThenByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
        return documents.Select(d => _mapper.Map<SigningKey>(d)).ToList();
    }

    public async Task DeactivateAllExceptAsync(long keyId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SigningKeyMongo>.Filter.And(
            Builders<SigningKeyMongo>.Filter.Ne(k => k.DomainId, keyId),
            Builders<SigningKeyMongo>.Filter.Eq(k => k.IsActive, true),
            Builders<SigningKeyMongo>.Filter.Eq(k => k.IsDeleted, false));
        var update = Builders<SigningKeyMongo>.Update
            .Set(k => k.IsActive, false)
            .Set(k => k.UpdatedAt, DateTime.UtcNow);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SigningKey>> GetExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var documents = await _collection
            .Find(k => !k.IsDeleted && k.ExpiresAt != null && k.ExpiresAt < now)
            .ToListAsync(cancellationToken);
        return documents.Select(d => _mapper.Map<SigningKey>(d)).ToList();
    }

    public async Task<IReadOnlyList<SigningKey>> GetKeysExpiringWithinAsync(
        TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiryThreshold = now.Add(timeSpan);
        var documents = await _collection
            .Find(k => !k.IsDeleted && !k.IsRevoked &&
                       k.ExpiresAt != null && k.ExpiresAt > now && k.ExpiresAt <= expiryThreshold)
            .ToListAsync(cancellationToken);
        return documents.Select(d => _mapper.Map<SigningKey>(d)).ToList();
    }
}