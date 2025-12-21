using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IRoleRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(PostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Roles.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Roles.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await Context.Roles.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        await Context.Roles.AddAsync(entity, cancellationToken);
    }

    public void Update(Role entity)
    {
        var trackedEntry = Context.ChangeTracker.Entries<Role>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            Context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            Context.Roles.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(Role entity)
    {
        Context.Roles.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Roles.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await Context.Roles
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetByRankGreaterThanAsync(int rank,
        CancellationToken cancellationToken = default)
    {
        return await Context.Roles
            .Where(r => r.Rank > rank)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Role> query = Context.Roles.Where(r => r.Code == code);
        if (excludeRoleId.HasValue) query = query.Where(r => r.Id != excludeRoleId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Role> Items, long Total)> GetPagedAsync(
        Expression<Func<Role, bool>>? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Expression<Func<Role, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Build base query
        IQueryable<Role> countQuery = Context.Roles.AsQueryable();
        IQueryable<Role> dataQuery = Context.Roles.AsQueryable();

        // Apply includes if provided
        if (includes != null && includes.Any())
        {
            foreach (var include in includes)
            {
                var propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }
        }

        // Apply filter at database level
        if (filter != null)
        {
            countQuery = countQuery.Where(filter);
            dataQuery = dataQuery.Where(filter);
        }

        // Get total count
        var total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, r => r.Rank);

        // Apply pagination and execute
        var roles = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (roles, total);
    }

    private Expression<Func<Role, object>> GetSortExpression(string fieldName)
    {
        return fieldName switch
        {
            "id" => r => r.Id,
            "code" => r => r.Code,
            "name" => r => r.Name,
            "description" => r => r.Description ?? "",
            "rank" => r => r.Rank,
            "issystemrole" => r => r.IsSystemRole,
            "createdat" => r => r.CreatedAt,
            "updatedat" => r => r.UpdatedAt ?? DateTime.MinValue,
            _ => throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting")
        };
    }
}
