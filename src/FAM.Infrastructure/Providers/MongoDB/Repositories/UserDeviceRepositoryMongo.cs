using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;
using FAM.Infrastructure.PersistenceModels.Mongo;

using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of IUserDeviceRepository
/// </summary>
public class UserDeviceRepositoryMongo : IUserDeviceRepository
{
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<UserDeviceMongo> _collection;

    public UserDeviceRepositoryMongo(MongoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _collection = _context.GetCollection<UserDeviceMongo>("userDevices");
    }

    public async Task<UserDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        UserDeviceMongo? document = await _collection.Find(d => d.DomainId == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<UserDevice>(document) : null;
    }

    public async Task<IEnumerable<UserDevice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<UserDeviceMongo>? documents = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDevice>>(documents);
    }

    public async Task<IEnumerable<UserDevice>> FindAsync(Expression<Func<UserDevice, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Note: Converting domain expressions to MongoDB queries is complex
        // For now, we'll get all and filter in memory
        List<UserDeviceMongo>? allDocuments = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        IEnumerable<UserDevice>? allEntities = _mapper.Map<IEnumerable<UserDevice>>(allDocuments);
        return allEntities.Where(predicate.Compile());
    }

    public async Task AddAsync(UserDevice entity, CancellationToken cancellationToken = default)
    {
        UserDeviceMongo? document = _mapper.Map<UserDeviceMongo>(entity);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(UserDevice entity)
    {
        UserDeviceMongo? document = _mapper.Map<UserDeviceMongo>(entity);
        FilterDefinition<UserDeviceMongo>? filter = Builders<UserDeviceMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.ReplaceOneAsync(filter, document);
    }

    public void Update(UserDevice entity)
    {
        UpdateAsync(entity).GetAwaiter().GetResult();
    }

    public async Task DeleteAsync(UserDevice entity)
    {
        FilterDefinition<UserDeviceMongo>? filter = Builders<UserDeviceMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.DeleteOneAsync(filter);
    }

    public void Delete(UserDevice entity)
    {
        DeleteAsync(entity).GetAwaiter().GetResult();
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            d => d.DomainId == id && !d.IsDeleted, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        UserDeviceMongo? document = await _collection.Find(d => d.DeviceId == deviceId && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<UserDevice>(document) : null;
    }

    public async Task<IEnumerable<UserDevice>> GetByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        List<UserDeviceMongo>? documents = await _collection.Find(d => d.UserId == userId && !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDevice>>(documents);
    }

    public async Task<IEnumerable<UserDevice>> GetActiveDevicesByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        List<UserDeviceMongo>? documents = await _collection.Find(d => d.UserId == userId && d.IsActive && !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDevice>>(documents);
    }

    public async Task<bool> IsDeviceIdTakenAsync(string deviceId, Guid? excludeDeviceId = null,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<UserDeviceMongo>? filter = Builders<UserDeviceMongo>.Filter.And(
            Builders<UserDeviceMongo>.Filter.Eq(d => d.DeviceId, deviceId),
            Builders<UserDeviceMongo>.Filter.Eq(d => d.IsDeleted, false)
        );

        if (excludeDeviceId.HasValue)
            filter = Builders<UserDeviceMongo>.Filter.And(
                filter,
                Builders<UserDeviceMongo>.Filter.Ne((UserDeviceMongo d) => d.DomainId, excludeDeviceId.Value)
            );

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task DeactivateDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        FilterDefinition<UserDeviceMongo>? filter =
            Builders<UserDeviceMongo>.Filter.Eq((UserDeviceMongo d) => d.DomainId, deviceId);
        UpdateDefinition<UserDeviceMongo>? update = Builders<UserDeviceMongo>.Update
            .Set(d => d.IsActive, false)
            .Set(d => d.RefreshToken, null)
            .Set(d => d.RefreshTokenExpiresAt, null);
        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DeactivateAllUserDevicesAsync(long userId, string? excludeDeviceId = null,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<UserDeviceMongo>? filter = Builders<UserDeviceMongo>.Filter.And(
            Builders<UserDeviceMongo>.Filter.Eq(d => d.UserId, userId),
            Builders<UserDeviceMongo>.Filter.Eq(d => d.IsDeleted, false)
        );

        // Exclude device by DeviceId (fingerprint string), not by Guid ID
        if (!string.IsNullOrEmpty(excludeDeviceId))
            filter = Builders<UserDeviceMongo>.Filter.And(
                filter,
                Builders<UserDeviceMongo>.Filter.Ne(d => d.DeviceId, excludeDeviceId)
            );

        UpdateDefinition<UserDeviceMongo>? update = Builders<UserDeviceMongo>.Update
            .Set(d => d.IsActive, false)
            .Set(d => d.RefreshToken, null)
            .Set(d => d.RefreshTokenExpiresAt, null);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<UserDevice?> FindByRefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        UserDeviceMongo? document = await _collection.Find(d => d.RefreshToken == refreshToken && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<UserDevice>(document) : null;
    }

    public async Task<bool> UpdateLastActivityAsync(string deviceId, string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<UserDeviceMongo>? filter = Builders<UserDeviceMongo>.Filter.And(
            Builders<UserDeviceMongo>.Filter.Eq(d => d.DeviceId, deviceId),
            Builders<UserDeviceMongo>.Filter.Eq(d => d.IsActive, true),
            Builders<UserDeviceMongo>.Filter.Eq(d => d.IsDeleted, false)
        );

        UpdateDefinition<UserDeviceMongo>? update = Builders<UserDeviceMongo>.Update
            .Set(d => d.LastActivityAt, DateTime.UtcNow)
            .Set(d => d.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(ipAddress)) update = update.Set(d => d.IpAddress, ipAddress);

        UpdateResult? result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
}
