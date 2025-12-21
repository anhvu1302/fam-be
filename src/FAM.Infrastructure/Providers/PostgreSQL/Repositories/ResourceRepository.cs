using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IResourceRepository using Pragmatic Architecture.
/// Works directly with domain entities without mapping layer.
/// </summary>
public class ResourceRepository : IResourceRepository
{
    private readonly PostgreSqlDbContext _context;

    public ResourceRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<Resource?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Resources.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Resources.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> FindAsync(Expression<Func<Resource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Resources.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Resource entity, CancellationToken cancellationToken = default)
    {
        await _context.Resources.AddAsync(entity, cancellationToken);
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
            _context.Resources.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(Resource entity)
    {
        _context.Resources.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Resources.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Resource?> GetByTypeAndNodeIdAsync(string type, long nodeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .FirstOrDefaultAsync(r => r.Type == type && r.NodeId == nodeId, cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetByNodeIdAsync(long nodeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .Where(r => r.NodeId == nodeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .Where(r => r.Type == type)
            .ToListAsync(cancellationToken);
    }
}
