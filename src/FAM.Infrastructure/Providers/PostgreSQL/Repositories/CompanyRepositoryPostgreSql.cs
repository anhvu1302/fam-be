using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Companies;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of ICompanyRepository
/// </summary>
public class CompanyRepositoryPostgreSql : ICompanyRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public CompanyRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<Company>(entity) : null;
    }

    public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Companies.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Company>>(entities);
    }

    public async Task<IEnumerable<Company>> FindAsync(Expression<Func<Company, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Convert domain predicate to EF predicate
        var efPredicate = _mapper.Map<Expression<Func<CompanyEf, bool>>>(predicate);
        var entities = await _context.Companies.Where(efPredicate).ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Company>>(entities);
    }

    public async Task AddAsync(Company entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<CompanyEf>(entity);
        await _context.Companies.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Company entity)
    {
        var efEntity = _mapper.Map<CompanyEf>(entity);
        _context.Companies.Update(efEntity);
    }

    public void Delete(Company entity)
    {
        var efEntity = _mapper.Map<CompanyEf>(entity);
        _context.Companies.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Companies.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies
            .FirstOrDefaultAsync(c => c.TaxCode == taxCode, cancellationToken);
        return entity != null ? _mapper.Map<Company>(entity) : null;
    }

    public async Task<IEnumerable<Company>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Companies
            .Where(c => c.Name.Contains(name))
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Company>>(entities);
    }

    public async Task<bool> IsNameTakenAsync(string name, long? excludeCompanyId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Companies.Where(c => c.Name == name);
        if (excludeCompanyId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCompanyId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }
}