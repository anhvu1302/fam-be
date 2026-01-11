using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IRolePermissionRepository using Pragmatic Architecture.
/// Works with RolePermission domain entity directly with EF Core.
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly IDbContext _context;
    protected DbSet<RolePermission> DbSet => _context.Set<RolePermission>();

    public RolePermissionRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> FindAsync(Expression<Func<RolePermission, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RolePermission entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Delete(RolePermission entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long roleId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(long permissionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<RolePermission?> GetByRoleAndPermissionAsync(long roleId, long permissionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task DeleteByRoleIdAsync(long roleId, CancellationToken cancellationToken = default)
    {
        List<RolePermission> entities = await DbSet
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(entities);
    }
}
