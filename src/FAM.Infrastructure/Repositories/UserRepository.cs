using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class UserRepository : BasePagedRepository<User>, IUserRepository
{
    private readonly IDbContext _context;
    protected DbSet<User> DbSet => _context.Set<User>();

    public UserRepository(IDbContext context)
        : base()
    {
        _context = context;
    }


    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(User entity)
    {
        EntityEntry<User>? trackedEntry = _context.ChangeTracker.Entries<User>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            DbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(User entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = DbSet.Where(u => u.Username == username);
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsEmailTakenAsync(string email, long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.ToLower();
        IQueryable<User> query = DbSet.Where(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        string normalizedUsername = username.ToLower();
        return await DbSet
            .Include(u => u.UserDevices)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.ToLower();
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> FindByIdentityAsync(string identity, CancellationToken cancellationToken = default)
    {
        string normalizedInput = identity.ToLower();

        return await DbSet
            .Include(u => u.UserDevices)
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedInput || u.Email.ToLower() == normalizedInput,
                cancellationToken);
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
        IQueryable<User> countQuery = DbSet.AsQueryable();

        // Build query for data
        IQueryable<User> dataQuery = DbSet.AsQueryable();

        // Apply includes dynamically if provided
        if (includes != null && includes.Length > 0)
        {
            foreach (Expression<Func<User, object>> include in includes)
            {
                string propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }
        }

        // Apply filter at database level to both queries
        if (filter != null)
        {
            countQuery = countQuery.Where(filter);
            dataQuery = dataQuery.Where(filter);
        }

        // Get total count from count query
        int total = await countQuery.CountAsync(cancellationToken);

        // Apply sorting to data query (default to CreatedAt descending if not specified)
        if (string.IsNullOrWhiteSpace(sort))
        {
            dataQuery = dataQuery.OrderByDescending(u => u.CreatedAt);
        }
        else
        {
            // TODO: Implement dynamic sorting based on sort parameter if needed
            dataQuery = dataQuery.OrderByDescending(u => u.CreatedAt);
        }

        // Apply pagination and execute
        List<User> users = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (users, total);
    }
}
