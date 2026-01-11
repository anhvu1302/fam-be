using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Organizations;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL implementation of ICompanyDetailsRepository using Pragmatic Architecture.
/// Works directly with domain entities without mapping layer.
/// Follows Clean Architecture by depending on IDbContext.
/// </summary>
public class CompanyDetailsRepository : ICompanyDetailsRepository
{
    private readonly IDbContext _context;
    protected DbSet<CompanyDetails> DbSet => _context.Set<CompanyDetails>();

    public CompanyDetailsRepository(IDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDetails?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<CompanyDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CompanyDetails>> FindAsync(Expression<Func<CompanyDetails, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CompanyDetails entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(CompanyDetails entity)
    {
        EntityEntry<CompanyDetails>? trackedEntry = _context.ChangeTracker.Entries<CompanyDetails>()
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

    public void Delete(CompanyDetails entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(cd => cd.Id == id, cancellationToken);
    }

    public async Task<CompanyDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(cd => cd.NodeId == nodeId, cancellationToken);
    }

    public async Task<CompanyDetails?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(cd => cd.TaxCode == taxCode, cancellationToken);
    }

    public async Task<CompanyDetails?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(cd => cd.Domain == domain, cancellationToken);
    }
}
