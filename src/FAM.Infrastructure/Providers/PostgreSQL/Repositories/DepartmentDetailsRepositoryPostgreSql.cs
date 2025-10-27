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
/// PostgreSQL implementation of IDepartmentDetailsRepository
/// </summary>
public class DepartmentDetailsRepositoryPostgreSql : IDepartmentDetailsRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public DepartmentDetailsRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DepartmentDetails?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DepartmentDetails.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<DepartmentDetails>(entity) : null;
    }

    public async Task<IEnumerable<DepartmentDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.DepartmentDetails.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<DepartmentDetails>>(entities);
    }

    public async Task<IEnumerable<DepartmentDetails>> FindAsync(Expression<Func<DepartmentDetails, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.DepartmentDetails.ToListAsync(cancellationToken);
        var allDepartmentDetails = _mapper.Map<IEnumerable<DepartmentDetails>>(allEntities);
        return allDepartmentDetails.Where(predicate.Compile());
    }

    public async Task AddAsync(DepartmentDetails entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<DepartmentDetailsEf>(entity);
        await _context.DepartmentDetails.AddAsync(efEntity, cancellationToken);
    }

    public void Update(DepartmentDetails entity)
    {
        var efEntity = _mapper.Map<DepartmentDetailsEf>(entity);
        _context.DepartmentDetails.Update(efEntity);
    }

    public void Delete(DepartmentDetails entity)
    {
        var efEntity = _mapper.Map<DepartmentDetailsEf>(entity);
        _context.DepartmentDetails.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.DepartmentDetails.AnyAsync(dd => dd.Id == id, cancellationToken);
    }

    public async Task<DepartmentDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DepartmentDetails
            .FirstOrDefaultAsync(dd => dd.NodeId == nodeId, cancellationToken);
        return entity != null ? _mapper.Map<DepartmentDetails>(entity) : null;
    }

    public async Task<DepartmentDetails?> GetByCostCenterAsync(string costCenter, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DepartmentDetails
            .FirstOrDefaultAsync(dd => dd.CostCenter == costCenter, cancellationToken);
        return entity != null ? _mapper.Map<DepartmentDetails>(entity) : null;
    }

    public async Task<IEnumerable<DepartmentDetails>> GetByParentNodeIdAsync(long parentNodeId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.DepartmentDetails
            .Join(_context.OrgNodes,
                dd => dd.NodeId,
                n => n.Id,
                (dd, n) => new { DepartmentDetails = dd, Node = n })
            .Where(x => x.Node.ParentId == parentNodeId)
            .Select(x => x.DepartmentDetails)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<DepartmentDetails>>(entities);
    }
}