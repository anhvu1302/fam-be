using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Organizations;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL implementation of IDepartmentDetailsRepository using Pragmatic Architecture.
/// Works directly with domain entities without mapping layer.
/// Follows Clean Architecture by depending on IDbContext.
/// </summary>
public class DepartmentDetailsRepository : IDepartmentDetailsRepository
{
    private readonly IDbContext _context;
    protected DbSet<DepartmentDetails> DbSet => _context.Set<DepartmentDetails>();

    public DepartmentDetailsRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDetails?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<DepartmentDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DepartmentDetails>> FindAsync(Expression<Func<DepartmentDetails, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DepartmentDetails entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(DepartmentDetails entity)
    {
        EntityEntry<DepartmentDetails>? trackedEntry = _context.ChangeTracker.Entries<DepartmentDetails>()
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

    public void Delete(DepartmentDetails entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(dd => dd.Id == id, cancellationToken);
    }

    public async Task<DepartmentDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(dd => dd.NodeId == nodeId, cancellationToken);
    }

    public async Task<DepartmentDetails?> GetByCostCenterAsync(string costCenter,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(dd => dd.CostCenter == costCenter, cancellationToken);
    }

    public async Task<IEnumerable<DepartmentDetails>> GetByParentNodeIdAsync(long parentNodeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Join(_context.Set<OrgNode>(),
                dd => dd.NodeId,
                n => n.Id,
                (dd, n) => new { DepartmentDetails = dd, Node = n })
            .Where(x => x.Node.ParentId == parentNodeId)
            .Select(x => x.DepartmentDetails)
            .ToListAsync(cancellationToken);
    }
}
