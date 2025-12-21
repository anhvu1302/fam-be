using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Organizations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IDepartmentDetailsRepository using Pragmatic Architecture.
/// Works directly with domain entities without mapping layer.
/// </summary>
public class DepartmentDetailsRepository : IDepartmentDetailsRepository
{
    private readonly PostgreSqlDbContext _context;

    public DepartmentDetailsRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDetails?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<DepartmentDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DepartmentDetails>> FindAsync(Expression<Func<DepartmentDetails, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DepartmentDetails entity, CancellationToken cancellationToken = default)
    {
        await _context.DepartmentDetails.AddAsync(entity, cancellationToken);
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
            _context.DepartmentDetails.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(DepartmentDetails entity)
    {
        _context.DepartmentDetails.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails.AnyAsync(dd => dd.Id == id, cancellationToken);
    }

    public async Task<DepartmentDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails
            .FirstOrDefaultAsync(dd => dd.NodeId == nodeId, cancellationToken);
    }

    public async Task<DepartmentDetails?> GetByCostCenterAsync(string costCenter,
        CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails
            .FirstOrDefaultAsync(dd => dd.CostCenter == costCenter, cancellationToken);
    }

    public async Task<IEnumerable<DepartmentDetails>> GetByParentNodeIdAsync(long parentNodeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails
            .Join(_context.OrgNodes,
                dd => dd.NodeId,
                n => n.Id,
                (dd, n) => new { DepartmentDetails = dd, Node = n })
            .Where(x => x.Node.ParentId == parentNodeId)
            .Select(x => x.DepartmentDetails)
            .ToListAsync(cancellationToken);
    }
}
