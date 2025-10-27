using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Organizations;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IOrgNodeRepository
/// </summary>
public class OrgNodeRepositoryPostgreSql : IOrgNodeRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public OrgNodeRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgNode?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OrgNodes.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<OrgNode>(entity) : null;
    }

    public async Task<IEnumerable<OrgNode>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.OrgNodes.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<OrgNode>>(entities);
    }

    public async Task<IEnumerable<OrgNode>> FindAsync(Expression<Func<OrgNode, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.OrgNodes.ToListAsync(cancellationToken);
        var allOrgNodes = _mapper.Map<IEnumerable<OrgNode>>(allEntities);
        return allOrgNodes.Where(predicate.Compile());
    }

    public async Task AddAsync(OrgNode entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<OrgNodeEf>(entity);
        await _context.OrgNodes.AddAsync(efEntity, cancellationToken);
    }

    public void Update(OrgNode entity)
    {
        var efEntity = _mapper.Map<OrgNodeEf>(entity);
        _context.OrgNodes.Update(efEntity);
    }

    public void Delete(OrgNode entity)
    {
        var efEntity = _mapper.Map<OrgNodeEf>(entity);
        _context.OrgNodes.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.AnyAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<OrgNode?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OrgNodes
            .FirstOrDefaultAsync(n => n.Name == name, cancellationToken);
        return entity != null ? _mapper.Map<OrgNode>(entity) : null;
    }

    public async Task<IEnumerable<OrgNode>> GetByParentIdAsync(long? parentId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.OrgNodes
            .Where(n => n.ParentId == parentId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<OrgNode>>(entities);
    }

    public async Task<IEnumerable<OrgNode>> GetByTypeAsync(OrgNodeType type, CancellationToken cancellationToken = default)
    {
        var entities = await _context.OrgNodes
            .Where(n => n.Type == (int)type)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<OrgNode>>(entities);
    }

    public async Task<IEnumerable<OrgNode>> GetHierarchyAsync(long rootNodeId, CancellationToken cancellationToken = default)
    {
        // This is a simplified implementation. In a real scenario, you might want to use a recursive CTE or other hierarchical query
        var root = await _context.OrgNodes.FindAsync(new object[] { rootNodeId }, cancellationToken);
        if (root == null) return new List<OrgNode>();

        var hierarchy = new List<OrgNodeEf> { root };
        await GetChildrenRecursiveAsync(root.Id, hierarchy, cancellationToken);

        return _mapper.Map<IEnumerable<OrgNode>>(hierarchy);
    }

    private async Task GetChildrenRecursiveAsync(long parentId, List<OrgNodeEf> hierarchy, CancellationToken cancellationToken)
    {
        var children = await _context.OrgNodes
            .Where(n => n.ParentId == parentId)
            .ToListAsync(cancellationToken);

        hierarchy.AddRange(children);

        foreach (var child in children)
        {
            await GetChildrenRecursiveAsync(child.Id, hierarchy, cancellationToken);
        }
    }

    public async Task<bool> HasChildrenAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        return await _context.OrgNodes.AnyAsync(n => n.ParentId == nodeId, cancellationToken);
    }
}