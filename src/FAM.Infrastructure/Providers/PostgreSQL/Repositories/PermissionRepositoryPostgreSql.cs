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
/// PostgreSQL implementation of IPermissionRepository
/// </summary>
public class PermissionRepositoryPostgreSql : IPermissionRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public PermissionRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<Permission>(entity) : null;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Permissions.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Permission>>(entities);
    }

    public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.Permissions.ToListAsync(cancellationToken);
        var allPermissions = _mapper.Map<IEnumerable<Permission>>(allEntities);
        return allPermissions.Where(predicate.Compile());
    }

    public async Task AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<PermissionEf>(entity);
        await _context.Permissions.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Permission entity)
    {
        var efEntity = _mapper.Map<PermissionEf>(entity);
        _context.Permissions.Update(efEntity);
    }

    public void Delete(Permission entity)
    {
        var efEntity = _mapper.Map<PermissionEf>(entity);
        _context.Permissions.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Permission?> GetByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Resource == resource && p.Action == action, cancellationToken);
        return entity != null ? _mapper.Map<Permission>(entity) : null;
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Permissions
            .Where(p => p.Resource == resource)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Permission>>(entities);
    }

    public async Task<bool> ExistsByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AnyAsync(p => p.Resource == resource && p.Action == action, cancellationToken);
    }
}