using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IResourceRepository using Pragmatic Architecture.
/// Works directly with domain entities without mapping layer.
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class ResourceRepository : IResourceRepository
{
    private readonly IDbContext _context;
    protected DbSet<Resource> DbSet => _context.Set<Resource>();

    public ResourceRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<Resource?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> FindAsync(Expression<Func<Resource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Resource entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(Resource entity)
    {
        EntityEntry<Resource>? trackedEntry = _context.ChangeTracker.Entries<Resource>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            DbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(Resource entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Resource?> GetByTypeAndNodeIdAsync(string type, long nodeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.Type == type && r.NodeId == nodeId, cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetByNodeIdAsync(long nodeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.NodeId == nodeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.Type == type)
            .ToListAsync(cancellationToken);
    }
}
