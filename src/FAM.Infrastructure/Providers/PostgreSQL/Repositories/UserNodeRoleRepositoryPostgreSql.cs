using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserNodeRoleRepository
/// </summary>
public class UserNodeRoleRepositoryPostgreSql : IUserNodeRoleRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public UserNodeRoleRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserNodeRole?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserNodeRoles.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<UserNodeRole>(entity) : null;
    }

    public async Task<IEnumerable<UserNodeRole>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserNodeRoles.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserNodeRole>>(entities);
    }

    public async Task<IEnumerable<UserNodeRole>> FindAsync(Expression<Func<UserNodeRole, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.UserNodeRoles.ToListAsync(cancellationToken);
        var allUserNodeRoles = _mapper.Map<IEnumerable<UserNodeRole>>(allEntities);
        return allUserNodeRoles.Where(predicate.Compile());
    }

    public async Task AddAsync(UserNodeRole entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<UserNodeRoleEf>(entity);
        await _context.UserNodeRoles.AddAsync(efEntity, cancellationToken);
    }

    public void Update(UserNodeRole entity)
    {
        var efEntity = _mapper.Map<UserNodeRoleEf>(entity);
        _context.UserNodeRoles.Update(efEntity);
    }

    public void Delete(UserNodeRole entity)
    {
        var efEntity = _mapper.Map<UserNodeRoleEf>(entity);
        _context.UserNodeRoles.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.UserNodeRoles.AnyAsync(unr => unr.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UserNodeRole>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserNodeRoles
            .Where(unr => unr.UserId == userId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserNodeRole>>(entities);
    }

    public async Task<IEnumerable<UserNodeRole>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserNodeRoles
            .Where(unr => unr.NodeId == nodeId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserNodeRole>>(entities);
    }

    public async Task<IEnumerable<UserNodeRole>> GetByRoleIdAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserNodeRoles
            .Where(unr => unr.RoleId == roleId)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserNodeRole>>(entities);
    }

    public async Task<UserNodeRole?> GetByUserAndNodeAndRoleAsync(long userId, long nodeId, long roleId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserNodeRoles
            .FirstOrDefaultAsync(unr => unr.UserId == userId && unr.NodeId == nodeId && unr.RoleId == roleId, cancellationToken);
        return entity != null ? _mapper.Map<UserNodeRole>(entity) : null;
    }

    public async Task DeleteByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserNodeRoles
            .Where(unr => unr.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.UserNodeRoles.RemoveRange(entities);
    }

    public async Task DeleteByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserNodeRoles
            .Where(unr => unr.NodeId == nodeId)
            .ToListAsync(cancellationToken);

        _context.UserNodeRoles.RemoveRange(entities);
    }
}