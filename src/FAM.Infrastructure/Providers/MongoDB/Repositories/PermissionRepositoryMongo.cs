using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Repositories;

using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of IPermissionRepository
/// Inherits from BaseMongoRepository for common query operations
/// </summary>
public class PermissionRepositoryMongo : BaseMongoRepository<Permission, PermissionMongo>, IPermissionRepository
{
    private readonly IMongoCollection<PermissionMongo> _collection;

    public PermissionRepositoryMongo(MongoDbContext context, IMapper mapper) : base(context, mapper)
    {
        _collection = Context.GetCollection<PermissionMongo>("permissions");
    }

    public async Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        PermissionMongo? document = await _collection.Find(d => d.DomainId == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? Mapper.Map<Permission>(document) : null;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<PermissionMongo>? documents = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Permission>>(documents);
    }

    public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<PermissionMongo>? allDocuments = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        IEnumerable<Permission>? allEntities = Mapper.Map<IEnumerable<Permission>>(allDocuments);
        return allEntities.Where(predicate.Compile());
    }

    public async Task AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        PermissionMongo? document = Mapper.Map<PermissionMongo>(entity);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Permission entity)
    {
        PermissionMongo? document = Mapper.Map<PermissionMongo>(entity);
        FilterDefinition<PermissionMongo>? filter = Builders<PermissionMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.ReplaceOneAsync(filter, document);
    }

    public void Update(Permission entity)
    {
        UpdateAsync(entity).GetAwaiter().GetResult();
    }

    public async Task DeleteAsync(Permission entity)
    {
        FilterDefinition<PermissionMongo>? filter = Builders<PermissionMongo>.Filter.Eq(d => d.DomainId, entity.Id);
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

    public async Task<Permission?> GetByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        PermissionMongo? document = await _collection
            .Find(d => d.Resource == resource && d.Action == action && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? Mapper.Map<Permission>(document) : null;
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource,
        CancellationToken cancellationToken = default)
    {
        List<PermissionMongo>? documents = await _collection.Find(d => d.Resource == resource && !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Permission>>(documents);
    }

    public async Task<bool> ExistsByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            d => d.Resource == resource && d.Action == action && !d.IsDeleted, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<(IEnumerable<Permission> Items, long Total)> GetPagedAsync(
        Expression<Func<Permission, bool>>? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Expression<Func<Permission, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Start with base filter for non-deleted documents
        FilterDefinitionBuilder<PermissionMongo>? filterBuilder = Builders<PermissionMongo>.Filter;
        FilterDefinition<PermissionMongo>? mongoFilter = filterBuilder.Eq(d => d.IsDeleted, false);

        // Apply domain filter at database level
        if (filter != null)
        {
            Expression<Func<PermissionMongo, bool>> efFilter = ConvertToEfExpression(filter);
            // Convert EF expression to MongoDB filter (simplified approach)
            // For complex filters, may need to load and filter in memory
            List<PermissionMongo>? allDocuments = await _collection.Find(mongoFilter).ToListAsync(cancellationToken);
            IEnumerable<Permission>? allEntities = Mapper.Map<IEnumerable<Permission>>(allDocuments);
            var filtered = allEntities.Where(filter.Compile()).ToList();
            var filteredIds = filtered.Select(p => p.Id).ToHashSet();

            mongoFilter = filterBuilder.And(
                mongoFilter,
                filterBuilder.In(d => d.DomainId, filteredIds)
            );
        }

        // Build MongoDB query
        IFindFluent<PermissionMongo, PermissionMongo>? query = _collection.Find(mongoFilter);

        // Apply sorting using base method
        SortDefinition<PermissionMongo>? sortDefinition = ApplySort(sort, GetSortDefinition);
        if (sortDefinition != null)
            query = query.Sort(sortDefinition);
        else
            // Default sorting
            query = query.Sort(Builders<PermissionMongo>.Sort.Ascending(d => d.DomainId));

        // Get total count
        var total = await _collection.CountDocumentsAsync(mongoFilter, cancellationToken: cancellationToken);

        // Apply pagination and execute
        List<PermissionMongo>? documents = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        // Map to domain entities
        List<Permission>? permissions = Mapper.Map<List<Permission>>(documents);

        return (permissions, total);
    }

    private SortDefinition<PermissionMongo> GetSortDefinition(string fieldName)
    {
        SortDefinitionBuilder<PermissionMongo>? sortBuilder = Builders<PermissionMongo>.Sort;
        var descending = fieldName.StartsWith('-');
        var actualFieldName = descending ? fieldName[1..] : fieldName;

        return actualFieldName switch
        {
            "id" => descending ? sortBuilder.Descending(d => d.DomainId) : sortBuilder.Ascending(d => d.DomainId),
            "resource" => descending ? sortBuilder.Descending(d => d.Resource) : sortBuilder.Ascending(d => d.Resource),
            "action" => descending ? sortBuilder.Descending(d => d.Action) : sortBuilder.Ascending(d => d.Action),
            "description" => descending
                ? sortBuilder.Descending(d => d.Description)
                : sortBuilder.Ascending(d => d.Description),
            "createdat" => descending
                ? sortBuilder.Descending(d => d.CreatedAt)
                : sortBuilder.Ascending(d => d.CreatedAt),
            "updatedat" => descending
                ? sortBuilder.Descending(d => d.UpdatedAt)
                : sortBuilder.Ascending(d => d.UpdatedAt),
            _ => throw new InvalidOperationException($"Field '{actualFieldName}' cannot be used for sorting")
        };
    }
}
