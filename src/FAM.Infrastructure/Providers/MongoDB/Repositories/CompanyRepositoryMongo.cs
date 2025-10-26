using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Companies;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Providers.MongoDB;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of ICompanyRepository
/// </summary>
public class CompanyRepositoryMongo : ICompanyRepository
{
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<CompanyMongo> _collection;

    public CompanyRepositoryMongo(MongoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _collection = _context.GetCollection<CompanyMongo>("companies");
    }

    public async Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.DomainId == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<Company>(document) : null;
    }

    public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Company>>(documents);
    }

    public async Task<IEnumerable<Company>> FindAsync(Expression<Func<Company, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Note: Converting domain expressions to MongoDB queries is complex
        // For now, we'll get all and filter in memory
        var allDocuments = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        var allEntities = _mapper.Map<IEnumerable<Company>>(allDocuments);
        return allEntities.Where(predicate.Compile());
    }

    public async Task AddAsync(Company entity, CancellationToken cancellationToken = default)
    {
        var document = _mapper.Map<CompanyMongo>(entity);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Company entity)
    {
        var document = _mapper.Map<CompanyMongo>(entity);
        var filter = Builders<CompanyMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.ReplaceOneAsync(filter, document);
    }

    public async Task DeleteAsync(Company entity)
    {
        var filter = Builders<CompanyMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.DeleteOneAsync(filter);
    }

    public void Update(Company entity)
    {
        UpdateAsync(entity).GetAwaiter().GetResult();
    }

    public void Delete(Company entity)
    {
        DeleteAsync(entity).GetAwaiter().GetResult();
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            d => d.DomainId == id && !d.IsDeleted, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<Company?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.TaxCode == taxCode && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<Company>(document) : null;
    }

    public async Task<IEnumerable<Company>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CompanyMongo>.Filter.And(
            Builders<CompanyMongo>.Filter.Text(name),
            Builders<CompanyMongo>.Filter.Eq(d => d.IsDeleted, false)
        );
        var documents = await _collection.Find(filter).ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Company>>(documents);
    }

    public async Task<bool> IsNameTakenAsync(string name, long? excludeCompanyId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CompanyMongo>.Filter.And(
            Builders<CompanyMongo>.Filter.Eq(d => d.Name, name),
            Builders<CompanyMongo>.Filter.Eq(d => d.IsDeleted, false)
        );

        if (excludeCompanyId.HasValue)
        {
            filter = Builders<CompanyMongo>.Filter.And(
                filter,
                Builders<CompanyMongo>.Filter.Ne(d => d.DomainId, excludeCompanyId.Value)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }
}