using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IRolePermissionRepository
/// </summary>
public class RolePermissionRepositoryPostgreSql : IRolePermissionRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public RolePermissionRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<RolePermissionEf> entities = await _context.RolePermissions.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<RolePermission>>(entities);
    }

    public async Task<IEnumerable<RolePermission>> FindAsync(Expression<Func<RolePermission, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<RolePermissionEf> allEntities = await _context.RolePermissions.ToListAsync(cancellationToken);
        IEnumerable<RolePermission>? allRolePermissions = _mapper.Map<IEnumerable<RolePermission>>(allEntities);
        return allRolePermissions.Where(predicate.Compile());
    }

    public async Task AddAsync(RolePermission entity, CancellationToken cancellationToken = default)
    {
        RolePermissionEf? efEntity = _mapper.Map<RolePermissionEf>(entity);
        await _context.RolePermissions.AddAsync(efEntity, cancellationToken);
    }

    public void Delete(RolePermission entity)
    {
        RolePermissionEf? efEntity = _mapper.Map<RolePermissionEf>(entity);
        _context.RolePermissions.Remove(efEntity);
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long roleId,
        CancellationToken cancellationToken = default)
    {
        List<RolePermissionEf> entities = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<RolePermission>>(entities);
    }

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(long permissionId,
        CancellationToken cancellationToken = default)
    {
        List<RolePermissionEf> entities = await _context.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<RolePermission>>(entities);
    }

    public async Task<RolePermission?> GetByRoleAndPermissionAsync(long roleId, long permissionId,
        CancellationToken cancellationToken = default)
    {
        RolePermissionEf? entity = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
        return entity != null ? _mapper.Map<RolePermission>(entity) : null;
    }

    public async Task DeleteByRoleIdAsync(long roleId, CancellationToken cancellationToken = default)
    {
        List<RolePermissionEf> entities = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(entities);
    }
}
