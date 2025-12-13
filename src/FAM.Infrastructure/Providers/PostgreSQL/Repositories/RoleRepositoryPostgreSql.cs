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
/// PostgreSQL implementation of IRoleRepository
/// Inherits from BasePostgreSqlRepository for common query operations
/// </summary>
public class RoleRepositoryPostgreSql : BasePostgreSqlRepository<Role, RoleEf>, IRoleRepository
{
    public RoleRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        RoleEf? entity = await Context.Roles.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? Mapper.Map<Role>(entity) : null;
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<RoleEf> entities = await Context.Roles.ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Role>>(entities);
    }

    public async Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<RoleEf> allEntities = await Context.Roles.ToListAsync(cancellationToken);
        IEnumerable<Role>? allRoles = Mapper.Map<IEnumerable<Role>>(allEntities);
        return allRoles.Where(predicate.Compile());
    }

    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        RoleEf? efEntity = Mapper.Map<RoleEf>(entity);
        await Context.Roles.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Role entity)
    {
        RoleEf? efEntity = Mapper.Map<RoleEf>(entity);

        EntityEntry<RoleEf>? trackedEntry = Context.ChangeTracker.Entries<RoleEf>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            Context.Entry(trackedEntry.Entity).CurrentValues.SetValues(efEntity);
        }
        else
        {
            Context.Roles.Attach(efEntity);
            Context.Entry(efEntity).State = EntityState.Modified;
        }
    }

    public void Delete(Role entity)
    {
        RoleEf? efEntity = Mapper.Map<RoleEf>(entity);
        Context.Roles.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Roles.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        RoleEf? entity = await Context.Roles
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
        return entity != null ? Mapper.Map<Role>(entity) : null;
    }

    public async Task<IEnumerable<Role>> GetByRankGreaterThanAsync(int rank,
        CancellationToken cancellationToken = default)
    {
        List<RoleEf> entities = await Context.Roles
            .Where(r => r.Rank > rank)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Role>>(entities);
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<RoleEf> query = Context.Roles.Where(r => r.Code == code);
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
        // Build base query for counting
        IQueryable<RoleEf> countQuery = Context.Roles.AsQueryable();

        // Build query for data
        IQueryable<RoleEf> dataQuery = Context.Roles.AsQueryable();

        // Apply includes dynamically if provided
        if (includes != null && includes.Any())
            foreach (Expression<Func<Role, object>> include in includes)
            {
                var propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }

        // Apply filter at database level
        if (filter != null)
        {
            Expression<Func<RoleEf, bool>> efFilter = ConvertToEfExpression(filter);
            countQuery = countQuery.Where(efFilter);
            dataQuery = dataQuery.Where(efFilter);
        }

        // Get total count from count query
        var total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting to data query
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, r => r.Rank);

        // Apply pagination and execute
        List<RoleEf> entities = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to domain entities
        List<Role>? roles = Mapper.Map<List<Role>>(entities);

        return (roles, total);
    }

    private Expression<Func<RoleEf, object>> GetSortExpression(string fieldName)
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
