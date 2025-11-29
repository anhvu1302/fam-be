using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IRoleRepository
/// </summary>
public class RoleRepositoryPostgreSql : IRoleRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public RoleRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<Role>(entity) : null;
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Roles.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Role>>(entities);
    }

    public async Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.Roles.ToListAsync(cancellationToken);
        var allRoles = _mapper.Map<IEnumerable<Role>>(allEntities);
        return allRoles.Where(predicate.Compile());
    }

    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<RoleEf>(entity);
        await _context.Roles.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Role entity)
    {
        var efEntity = _mapper.Map<RoleEf>(entity);
        _context.Roles.Update(efEntity);
    }

    public void Delete(Role entity)
    {
        var efEntity = _mapper.Map<RoleEf>(entity);
        _context.Roles.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Roles
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
        return entity != null ? _mapper.Map<Role>(entity) : null;
    }

    public async Task<IEnumerable<Role>> GetByRankGreaterThanAsync(int rank,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Roles
            .Where(r => r.Rank > rank)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Role>>(entities);
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.Where(r => r.Code == code);
        if (excludeRoleId.HasValue) query = query.Where(r => r.Id != excludeRoleId.Value);
        return await query.AnyAsync(cancellationToken);
    }
}