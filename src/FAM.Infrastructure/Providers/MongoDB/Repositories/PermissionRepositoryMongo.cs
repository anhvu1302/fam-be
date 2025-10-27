using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Providers.MongoDB;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of IPermissionRepository
/// </summary>
public class PermissionRepositoryMongo : IPermissionRepository
{
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<PermissionMongo> _collection;

    public PermissionRepositoryMongo(MongoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _collection = _context.GetCollection<PermissionMongo>("permissions");
    }

    public async Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.DomainId == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<Permission>(document) : null;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Permission>>(documents);
    }

    public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var allDocuments = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        var allEntities = _mapper.Map<IEnumerable<Permission>>(allDocuments);
        return allEntities.Where(predicate.Compile());
    }

    public async Task AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        var document = _mapper.Map<PermissionMongo>(entity);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Permission entity)
    {
        var document = _mapper.Map<PermissionMongo>(entity);
        var filter = Builders<PermissionMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.ReplaceOneAsync(filter, document);
    }

    public void Update(Permission entity)
    {
        UpdateAsync(entity).GetAwaiter().GetResult();
    }

    public async Task DeleteAsync(Permission entity)
    {
        var filter = Builders<PermissionMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.DeleteOneAsync(filter);
    }

    public void Delete(Permission entity)
    {
        DeleteAsync(entity).GetAwaiter().GetResult();
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            d => d.DomainId == id && !d.IsDeleted, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<Permission?> GetByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.Resource == resource && d.Action == action && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<Permission>(document) : null;
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(d => d.Resource == resource && !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Permission>>(documents);
    }

    public async Task<bool> ExistsByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            d => d.Resource == resource && d.Action == action && !d.IsDeleted, cancellationToken: cancellationToken);
        return count > 0;
    }
}