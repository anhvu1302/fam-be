using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        ResourceEf? entity = await _context.Resources.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<Resource>(entity) : null;
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<ResourceEf> entities = await _context.Resources.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Resource>>(entities);
    }

    public async Task<IEnumerable<Resource>> FindAsync(Expression<Func<Resource, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<ResourceEf> allEntities = await _context.Resources.ToListAsync(cancellationToken);
        IEnumerable<Resource>? allResources = _mapper.Map<IEnumerable<Resource>>(allEntities);
        return allResources.Where(predicate.Compile());
    }

    public async Task AddAsync(Resource entity, CancellationToken cancellationToken = default)
    {
        ResourceEf? efEntity = _mapper.Map<ResourceEf>(entity);
        await _context.Resources.AddAsync(efEntity, cancellationToken);
    }

    public void Update(Resource entity)
    {
        ResourceEf? efEntity = _mapper.Map<ResourceEf>(entity);

        EntityEntry<ResourceEf>? trackedEntry = _context.ChangeTracker.Entries<ResourceEf>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(efEntity);
        }
        else
        {
            _context.Resources.Attach(efEntity);
            _context.Entry(efEntity).State = EntityState.Modified;
        }
    }

    public void Delete(Resource entity)
    {
        ResourceEf? efEntity = _mapper.Map<ResourceEf>(entity);
        _context.Resources.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Resources.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Resource?> GetByTypeAndNodeIdAsync(string type, long nodeId,
        CancellationToken cancellationToken = default)
    {
        ResourceEf? entity = await _context.Resources
            .FirstOrDefaultAsync(r => r.Type == type && r.NodeId == nodeId, cancellationToken);
        return entity != null ? _mapper.Map<Resource>(entity) : null;
    }

    public async Task<IEnumerable<Resource>> GetByNodeIdAsync(long nodeId,
        CancellationToken cancellationToken = default)
    {
        List<ResourceEf> entities = await _context.Resources
            .Where(r => r.NodeId == nodeId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Resource>>(entities);
    }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        List<ResourceEf> entities = await _context.Resources
            .Where(r => r.Type == type)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<Resource>>(entities);
    }
}
