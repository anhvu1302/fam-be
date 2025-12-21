using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Organizations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IOrgNodeRepository
/// </summary>
public class OrgNodeRepository : IOrgNodeRepository
{
    private readonly PostgreSqlDbContext _context;

    public OrgNodeRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<OrgNode?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<OrgNode>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrgNode>> FindAsync(Expression<Func<OrgNode, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrgNode entity, CancellationToken cancellationToken = default)
    {
        await _context.OrgNodes.AddAsync(entity, cancellationToken);
    }

    public void Update(OrgNode entity)
    {
        EntityEntry<OrgNode>? trackedEntry = _context.ChangeTracker.Entries<OrgNode>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _context.OrgNodes.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(OrgNode entity)
    {
        _context.OrgNodes.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.AnyAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<OrgNode?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes
            .FirstOrDefaultAsync(n => n.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<OrgNode>> GetByParentIdAsync(long? parentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes
            .Where(n => n.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrgNode>> GetByTypeAsync(OrgNodeType type,
        CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes
            .Where(n => n.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrgNode>> GetHierarchyAsync(long rootNodeId,
        CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation. In a real scenario, you might want to use a recursive CTE or other hierarchical query
        OrgNode? root = await _context.OrgNodes.FindAsync(new object[] { rootNodeId }, cancellationToken);
        if (root == null) return new List<OrgNode>();

        var hierarchy = new List<OrgNode> { root };
        await GetChildrenRecursiveAsync(root.Id, hierarchy, cancellationToken);

        return hierarchy;
    }

    private async Task GetChildrenRecursiveAsync(long parentId, List<OrgNode> hierarchy,
        CancellationToken cancellationToken)
    {
        List<OrgNode> children = await _context.OrgNodes
            .Where(n => n.ParentId == parentId)
            .ToListAsync(cancellationToken);

        hierarchy.AddRange(children);

        foreach (OrgNode child in children) await GetChildrenRecursiveAsync(child.Id, hierarchy, cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.AnyAsync(n => n.ParentId == nodeId, cancellationToken);
    }
}
