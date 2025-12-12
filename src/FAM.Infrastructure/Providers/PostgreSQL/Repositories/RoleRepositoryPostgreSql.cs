using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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
        var entity = await Context.Roles.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? Mapper.Map<Role>(entity) : null;
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Context.Roles.ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Role>>(entities);
    }

    public async Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await Context.Roles.ToListAsync(cancellationToken);
        var allRoles = Mapper.Map<IEnumerable<Role>>(allEntities);
        return allRoles.Where(predicate.Compile());
    }

    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        var efEntity = Mapper.Map<RoleEf>(entity);
        await Context.Roles.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Role entity)
    {
        var efEntity = Mapper.Map<RoleEf>(entity);

        var trackedEntry = Context.ChangeTracker.Entries<RoleEf>()
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
        var efEntity = Mapper.Map<RoleEf>(entity);
        Context.Roles.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.Roles.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Roles
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
        return entity != null ? Mapper.Map<Role>(entity) : null;
    }

    public async Task<IEnumerable<Role>> GetByRankGreaterThanAsync(int rank,
        CancellationToken cancellationToken = default)
    {
        var entities = await Context.Roles
            .Where(r => r.Rank > rank)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<Role>>(entities);
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Roles.Where(r => r.Code == code);
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
        var countQuery = Context.Roles.AsQueryable();

        // Build query for data
        var dataQuery = Context.Roles.AsQueryable();

        // Apply includes dynamically if provided
        if (includes != null && includes.Any())
            foreach (var include in includes)
            {
                var propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }

        // Apply filter at database level
        if (filter != null)
        {
            var efFilter = ConvertToEfExpression(filter);
            countQuery = countQuery.Where(efFilter);
            dataQuery = dataQuery.Where(efFilter);
        }

        // Get total count from count query
        var total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting to data query
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, r => r.Rank);

        // Apply pagination and execute
        var entities = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to domain entities
        var roles = Mapper.Map<List<Role>>(entities);

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