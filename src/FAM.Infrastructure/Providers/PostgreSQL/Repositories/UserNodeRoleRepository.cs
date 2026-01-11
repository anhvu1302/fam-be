using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserNodeRoleRepository using Pragmatic Architecture.
/// Works with UserNodeRole domain entity directly with EF Core.
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class UserNodeRoleRepository : IUserNodeRoleRepository
{
    private readonly IDbContext _context;
    protected DbSet<UserNodeRole> DbSet => _context.Set<UserNodeRole>();

    public UserNodeRoleRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserNodeRole>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserNodeRole>> FindAsync(Expression<Func<UserNodeRole, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserNodeRole entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Delete(UserNodeRole entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<IEnumerable<UserNodeRole>> GetByUserIdAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(unr => unr.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserNodeRole>> GetByNodeIdAsync(long nodeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(unr => unr.NodeId == nodeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserNodeRole>> GetByRoleIdAsync(long roleId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(unr => unr.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserNodeRole?> GetByUserAndNodeAndRoleAsync(long userId, long nodeId, long roleId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(unr => unr.UserId == userId && unr.NodeId == nodeId && unr.RoleId == roleId,
                cancellationToken);
    }

    public async Task DeleteByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        List<UserNodeRole> entities = await DbSet
            .Where(unr => unr.UserId == userId)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(entities);
    }

    public async Task DeleteByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        List<UserNodeRole> entities = await DbSet
            .Where(unr => unr.NodeId == nodeId)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(entities);
    }
}
