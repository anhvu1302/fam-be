using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Providers.MongoDB;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of IUserRepository
/// </summary>
public class UserRepositoryMongo : IUserRepository
{
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<UserMongo> _collection;

    public UserRepositoryMongo(MongoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _collection = _context.GetCollection<UserMongo>("users");
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.DomainId == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<User>(document) : null;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<User>>(documents);
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Note: Converting domain expressions to MongoDB queries is complex
        // For now, we'll get all and filter in memory
        var allDocuments = await _collection.Find(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
        var allEntities = _mapper.Map<IEnumerable<User>>(allDocuments);
        return allEntities.Where(predicate.Compile());
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        var document = _mapper.Map<UserMongo>(entity);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(User entity)
    {
        var document = _mapper.Map<UserMongo>(entity);
        var filter = Builders<UserMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.ReplaceOneAsync(filter, document);
    }

    public void Update(User entity)
    {
        UpdateAsync(entity).GetAwaiter().GetResult();
    }

    public async Task DeleteAsync(User entity)
    {
        var filter = Builders<UserMongo>.Filter.Eq(d => d.DomainId, entity.Id);
        await _collection.DeleteOneAsync(filter);
    }

    public void Delete(User entity)
    {
        DeleteAsync(entity).GetAwaiter().GetResult();
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(
            d => d.DomainId == id && !d.IsDeleted, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.Username == username && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<User>(document) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.Email == email && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<User>(document) : null;
    }

    public async Task<bool> IsUsernameTakenAsync(string username, long? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserMongo>.Filter.And(
            Builders<UserMongo>.Filter.Eq(d => d.Username, username),
            Builders<UserMongo>.Filter.Eq(d => d.IsDeleted, false)
        );

        if (excludeUserId.HasValue)
        {
            filter = Builders<UserMongo>.Filter.And(
                filter,
                Builders<UserMongo>.Filter.Ne(d => d.DomainId, excludeUserId.Value)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<bool> IsEmailTakenAsync(string email, long? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserMongo>.Filter.And(
            Builders<UserMongo>.Filter.Eq(d => d.Email, email),
            Builders<UserMongo>.Filter.Eq(d => d.IsDeleted, false)
        );

        if (excludeUserId.HasValue)
        {
            filter = Builders<UserMongo>.Filter.And(
                filter,
                Builders<UserMongo>.Filter.Ne(d => d.DomainId, excludeUserId.Value)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.Username.ToLower() == username.ToLower() && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<User>(document) : null;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(d => d.Email.ToLower() == email.ToLower() && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        return document != null ? _mapper.Map<User>(document) : null;
    }
}