using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserRepository
/// </summary>
public class UserRepositoryPostgreSql : BasePagedRepository<User, UserEf>, IUserRepository
{
    public UserRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
        : base(context, mapper)
    {
    }

    protected override DbSet<UserEf> DbSet => Context.Users;

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        UserEf? entity = await Context.Users.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? Mapper.Map<User>(entity) : null;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<UserEf> entities = await Context.Users.ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<User>>(entities);
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<UserEf> allEntities = await Context.Users.ToListAsync(cancellationToken);
        IEnumerable<User>? allUsers = Mapper.Map<IEnumerable<User>>(allEntities);
        return allUsers.Where(predicate.Compile());
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        UserEf? efEntity = Mapper.Map<UserEf>(entity);
        await Context.Users.AddAsync(efEntity, cancellationToken);
    }

    public void Update(User entity)
    {
        UserEf? efEntity = Mapper.Map<UserEf>(entity);

        // Check if the entity is already being tracked
        EntityEntry<UserEf>? existingEntry = Context.ChangeTracker.Entries<UserEf>()
            .FirstOrDefault(e => e.Entity.Id == efEntity.Id);

        if (existingEntry != null)
        {
            // Entity is already tracked, update its properties (excluding navigation properties)
            Context.Entry(existingEntry.Entity).CurrentValues.SetValues(efEntity);
            existingEntry.State = EntityState.Modified;
        }
        else
        {
            // Entity is not tracked, attach and update (but don't track UserDevices collection)
            Context.Users.Attach(efEntity);
            Context.Entry(efEntity).State = EntityState.Modified;

            // Don't cascade to UserDevices - they should be managed separately
            foreach (UserDeviceEf device in efEntity.UserDevices) Context.Entry(device).State = EntityState.Unchanged;
        }
    }

    public void Delete(User entity)
    {
        UserEf? efEntity = Mapper.Map<UserEf>(entity);
        Context.Users.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        UserEf? entity = await Context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        return entity != null ? Mapper.Map<User>(entity) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        UserEf? entity = await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        return entity != null ? Mapper.Map<User>(entity) : null;
    }

    public async Task<bool> IsUsernameTakenAsync(string username, long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserEf> query = Context.Users.Where(u => u.Username == username);
        if (excludeUserId.HasValue) query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsEmailTakenAsync(string email, long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserEf> query = Context.Users.Where(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        if (excludeUserId.HasValue) query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        UserEf? entity = await Context.Users
            .Include(u => u.UserDevices)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted, cancellationToken);
        return entity != null ? Mapper.Map<User>(entity) : null;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        UserEf? entity = await Context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted, cancellationToken);
        return entity != null ? Mapper.Map<User>(entity) : null;
    }

    public async Task<User?> FindByIdentityAsync(string identity, CancellationToken cancellationToken = default)
    {
        var normalizedInput = identity.ToLower();
        UserEf? entity = await Context.Users
            .Include(u => u.UserDevices)
            .FirstOrDefaultAsync(u =>
                    (u.Username.ToLower() == normalizedInput || u.Email.ToLower() == normalizedInput) && !u.IsDeleted,
                cancellationToken);
        return entity != null ? Mapper.Map<User>(entity) : null;
    }

    /// <summary>
    /// Get users with filtering, sorting and pagination applied at database level
    /// </summary>
    public async Task<(List<User> Items, int Total)> GetPagedAsync(
        Expression<Func<User, bool>>? filter,
        string? sort,
        int page,
        int pageSize,
        Expression<Func<User, object>>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Build base query for counting (no need Include for count)
        IQueryable<UserEf> countQuery = Context.Users.AsQueryable();

        // Build query for data
        IQueryable<UserEf> dataQuery = Context.Users.AsQueryable();

        // Apply includes dynamically if provided
        if (includes != null && includes.Length > 0)
            foreach (Expression<Func<User, object>> include in includes)
            {
                // Convert domain expression to property path string
                var propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }

        // Apply filter at database level to both queries
        if (filter != null)
        {
            // Convert domain expression to EF expression
            Expression<Func<UserEf, bool>> efFilter = ConvertToEfExpression(filter);
            countQuery = countQuery.Where(efFilter);
            dataQuery = dataQuery.Where(efFilter);
        }

        // Get total count from count query
        var total = await countQuery.CountAsync(cancellationToken);

        // Apply sorting to data query
        dataQuery = ApplySort(dataQuery, sort);

        // If no sort specified, default to CreatedAt descending
        if (string.IsNullOrWhiteSpace(sort)) dataQuery = dataQuery.OrderByDescending(u => u.CreatedAt);

        // Apply pagination and execute
        List<UserEf> entities = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to domain entities
        List<User>? users = Mapper.Map<List<User>>(entities);

        return (users, total);
    }
}
