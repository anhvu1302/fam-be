using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IPermissionRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(PostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Permissions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await Context.Permissions.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        await Context.Permissions.AddAsync(entity, cancellationToken);
    }

    public void Update(Permission entity)
    {
        EntityEntry<Permission>? trackedEntry = Context.ChangeTracker.Entries<Permission>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            Context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            Context.Permissions.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(Permission entity)
    {
        Context.Permissions.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Permissions.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Permission?> GetByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        // Query using backing fields that EF Core maps
        return await Context.Permissions
            .Where(p => EF.Property<string>(p, "_resource") == resource &&
                        EF.Property<string>(p, "_action") == action)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource,
        CancellationToken cancellationToken = default)
    {
        return await Context.Permissions
            .Where(p => EF.Property<string>(p, "_resource") == resource)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        return await Context.Permissions
            .AnyAsync(p => EF.Property<string>(p, "_resource") == resource &&
                           EF.Property<string>(p, "_action") == action,
                cancellationToken);
    }

    public async Task<(IEnumerable<Permission> Items, long Total)> GetPagedAsync(
        Expression<Func<Permission, bool>>? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Expression<Func<Permission, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Build base query
        IQueryable<Permission> countQuery = Context.Permissions.AsQueryable();
        IQueryable<Permission> dataQuery = Context.Permissions.AsQueryable();

        // Apply includes if provided
        if (includes != null && includes.Any())
            foreach (Expression<Func<Permission, object>> include in includes)
            {
                var propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
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
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, p => p.Id);

        // Apply pagination and execute
        List<Permission> permissions = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (permissions, total);
    }

    private Expression<Func<Permission, object>> GetSortExpression(string fieldName)
    {
        return fieldName switch
        {
            "id" => p => p.Id,
            "resource" => p => EF.Property<string>(p, "_resource"),
            "action" => p => EF.Property<string>(p, "_action"),
            "description" => p => p.Description ?? "",
            "createdat" => p => p.CreatedAt,
            "updatedat" => p => p.UpdatedAt ?? DateTime.MinValue,
            _ => throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting")
        };
    }
}
