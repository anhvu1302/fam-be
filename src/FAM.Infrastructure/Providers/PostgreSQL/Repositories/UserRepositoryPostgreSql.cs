using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserRepository
/// </summary>
public class UserRepositoryPostgreSql : IUserRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public UserRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<User>(entity) : null;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Users.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<User>>(entities);
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Convert domain predicate to EF predicate - simplified for now
        var allEntities = await _context.Users.ToListAsync(cancellationToken);
        var allUsers = _mapper.Map<IEnumerable<User>>(allEntities);
        return allUsers.Where(predicate.Compile());
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<UserEf>(entity);
        await _context.Users.AddAsync(efEntity, cancellationToken);
    }

    public void Update(User entity)
    {
        var efEntity = _mapper.Map<UserEf>(entity);
        _context.Users.Update(efEntity);
    }

    public void Delete(User entity)
    {
        var efEntity = _mapper.Map<UserEf>(entity);
        _context.Users.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        return entity != null ? _mapper.Map<User>(entity) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        return entity != null ? _mapper.Map<User>(entity) : null;
    }

    public async Task<bool> IsUsernameTakenAsync(string username, long? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Username == username);
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsEmailTakenAsync(string email, long? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Email == email);
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Get IQueryable for EF entities (not domain) - for advanced filtering
    /// Note: Returns EF entities which will be mapped to domain later
    /// </summary>
    public IQueryable<User> GetQueryable()
    {
        // We need to return domain User but work with EF under the hood
        // For simplicity, we'll use a workaround: query EF then map in-memory
        // A better approach would be to use projection expressions
        throw new NotImplementedException("Use GetQueryableEf() instead for proper EF Core querying");
    }

    /// <summary>
    /// Get IQueryable of EF entities for filtering
    /// </summary>
    public IQueryable<UserEf> GetQueryableEf()
    {
        return _context.Users.AsNoTracking();
    }

    /// <summary>
    /// Map EF entities to domain after querying
    /// </summary>
    public IEnumerable<User> MapToDomain(IEnumerable<UserEf> efEntities)
    {
        return _mapper.Map<IEnumerable<User>>(efEntities);
    }
}