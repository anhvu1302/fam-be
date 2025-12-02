using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IResourceRepository
/// </summary>
public class ResourceRepositoryPostgreSql : IResourceRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public ResourceRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Resource?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Resources.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<Resource>(entity) : null;
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Resources.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Resource>>(entities);
    }

    public async Task<IEnumerable<Resource>> FindAsync(Expression<Func<Resource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.Resources.ToListAsync(cancellationToken);
        var allResources = _mapper.Map<IEnumerable<Resource>>(allEntities);
        return allResources.Where(predicate.Compile());
    }

    public async Task AddAsync(Resource entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<ResourceEf>(entity);
        await _context.Resources.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Resource entity)
    {
        var efEntity = _mapper.Map<ResourceEf>(entity);
        _context.Resources.Update(efEntity);
    }

    public void Delete(Resource entity)
    {
        var efEntity = _mapper.Map<ResourceEf>(entity);
        _context.Resources.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Resources.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Resource?> GetByTypeAndNodeIdAsync(string type, long nodeId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Resources
            .FirstOrDefaultAsync(r => r.Type == type && r.NodeId == nodeId, cancellationToken);
        return entity != null ? _mapper.Map<Resource>(entity) : null;
    }

    public async Task<IEnumerable<Resource>> GetByNodeIdAsync(long nodeId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Resources
            .Where(r => r.NodeId == nodeId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Resource>>(entities);
    }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Resources
            .Where(r => r.Type == type)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Resource>>(entities);
    }
}