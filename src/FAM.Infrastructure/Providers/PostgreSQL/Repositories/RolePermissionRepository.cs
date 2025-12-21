using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IRolePermissionRepository using Pragmatic Architecture.
/// Works with RolePermission domain entity directly with EF Core.
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly PostgreSqlDbContext _context;

    public RolePermissionRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> FindAsync(Expression<Func<RolePermission, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RolePermission entity, CancellationToken cancellationToken = default)
    {
        await _context.RolePermissions.AddAsync(entity, cancellationToken);
    }

    public void Delete(RolePermission entity)
    {
        _context.RolePermissions.Remove(entity);
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long roleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(long permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<RolePermission?> GetByRoleAndPermissionAsync(long roleId, long permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task DeleteByRoleIdAsync(long roleId, CancellationToken cancellationToken = default)
    {
        List<RolePermission> entities = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(entities);
    }
}
