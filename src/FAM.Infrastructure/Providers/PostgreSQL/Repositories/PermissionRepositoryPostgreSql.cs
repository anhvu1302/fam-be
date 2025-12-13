using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IPermissionRepository
/// Inherits from BasePostgreSqlRepository for common query operations
/// </summary>
public class PermissionRepositoryPostgreSql : BasePostgreSqlRepository<Permission, PermissionEf>, IPermissionRepository
{
    public PermissionRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        PermissionEf? entity = await Context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? Mapper.Map<Permission>(entity) : null;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<PermissionEf> entities = await Context.Permissions.ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Permission>>(entities);
    }

    public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<PermissionEf> allEntities = await Context.Permissions.ToListAsync(cancellationToken);
        IEnumerable<Permission>? allPermissions = Mapper.Map<IEnumerable<Permission>>(allEntities);
        return allPermissions.Where(predicate.Compile());
    }

    public async Task AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        PermissionEf? efEntity = Mapper.Map<PermissionEf>(entity);
        await Context.Permissions.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Permission entity)
    {
        PermissionEf? efEntity = Mapper.Map<PermissionEf>(entity);

        EntityEntry<PermissionEf>? trackedEntry = Context.ChangeTracker.Entries<PermissionEf>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            Context.Entry(trackedEntry.Entity).CurrentValues.SetValues(efEntity);
        }
        else
        {
            Context.Permissions.Attach(efEntity);
            Context.Entry(efEntity).State = EntityState.Modified;
        }
    }

    public void Delete(Permission entity)
    {
        PermissionEf? efEntity = Mapper.Map<PermissionEf>(entity);
        Context.Permissions.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Permissions.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Permission?> GetByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        PermissionEf? entity = await Context.Permissions
            .FirstOrDefaultAsync(p => p.Resource == resource && p.Action == action, cancellationToken);
        return entity != null ? Mapper.Map<Permission>(entity) : null;
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource,
        CancellationToken cancellationToken = default)
    {
        List<PermissionEf> entities = await Context.Permissions
            .Where(p => p.Resource == resource)
            .ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Permission>>(entities);
    }

    public async Task<bool> ExistsByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default)
    {
        return await Context.Permissions
            .AnyAsync(p => p.Resource == resource && p.Action == action, cancellationToken);
    }

    public async Task<(IEnumerable<Permission> Items, long Total)> GetPagedAsync(
        Expression<Func<Permission, bool>>? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Expression<Func<Permission, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Build base query for counting
        IQueryable<PermissionEf> countQuery = Context.Permissions.AsQueryable();

        // Build query for data
        IQueryable<PermissionEf> dataQuery = Context.Permissions.AsQueryable();

        // Apply includes dynamically if provided
        if (includes != null && includes.Any())
            foreach (Expression<Func<Permission, object>> include in includes)
            {
                var propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }

        // Apply filter at database level
        if (filter != null)
        {
            Expression<Func<PermissionEf, bool>> efFilter = ConvertToEfExpression(filter);
            countQuery = countQuery.Where(efFilter);
            dataQuery = dataQuery.Where(efFilter);
        }

        // Get total count from count query
        var total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting to data query
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, p => p.Id);

        // Apply pagination and execute
        List<PermissionEf> entities = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to domain entities
        List<Permission>? permissions = Mapper.Map<List<Permission>>(entities);

        return (permissions, total);
    }

    private Expression<Func<PermissionEf, object>> GetSortExpression(string fieldName)
    {
        return fieldName switch
        {
            "id" => p => p.Id,
            "resource" => p => p.Resource,
            "action" => p => p.Action,
            "description" => p => p.Description ?? "",
            "createdat" => p => p.CreatedAt,
            "updatedat" => p => p.UpdatedAt ?? DateTime.MinValue,
            _ => throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting")
        };
    }
}
