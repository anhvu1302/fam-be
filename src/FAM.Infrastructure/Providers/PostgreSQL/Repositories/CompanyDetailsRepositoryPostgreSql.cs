using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Organizations;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of ICompanyDetailsRepository
/// </summary>
public class CompanyDetailsRepositoryPostgreSql : ICompanyDetailsRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public CompanyDetailsRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyDetails?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CompanyDetails.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<CompanyDetails>(entity) : null;
    }

    public async Task<IEnumerable<CompanyDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.CompanyDetails.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CompanyDetails>>(entities);
    }

    public async Task<IEnumerable<CompanyDetails>> FindAsync(Expression<Func<CompanyDetails, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.CompanyDetails.ToListAsync(cancellationToken);
        var allCompanyDetails = _mapper.Map<IEnumerable<CompanyDetails>>(allEntities);
        return allCompanyDetails.Where(predicate.Compile());
    }

    public async Task AddAsync(CompanyDetails entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<CompanyDetailsEf>(entity);
        await _context.CompanyDetails.AddAsync(efEntity, cancellationToken);
    }

    public void Update(CompanyDetails entity)
    {
        var efEntity = _mapper.Map<CompanyDetailsEf>(entity);
        _context.CompanyDetails.Update(efEntity);
    }

    public void Delete(CompanyDetails entity)
    {
        var efEntity = _mapper.Map<CompanyDetailsEf>(entity);
        _context.CompanyDetails.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.CompanyDetails.AnyAsync(cd => cd.Id == id, cancellationToken);
    }

    public async Task<CompanyDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CompanyDetails
            .FirstOrDefaultAsync(cd => cd.NodeId == nodeId, cancellationToken);
        return entity != null ? _mapper.Map<CompanyDetails>(entity) : null;
    }

    public async Task<CompanyDetails?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CompanyDetails
            .FirstOrDefaultAsync(cd => cd.TaxCode == taxCode, cancellationToken);
        return entity != null ? _mapper.Map<CompanyDetails>(entity) : null;
    }

    public async Task<CompanyDetails?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CompanyDetails
            .FirstOrDefaultAsync(cd => cd.Domain == domain, cancellationToken);
        return entity != null ? _mapper.Map<CompanyDetails>(entity) : null;
    }
}