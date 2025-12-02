using FAM.Domain.Authorization;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface for Permission entity
/// </summary>
public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default);

    Task<bool> ExistsByResourceAndActionAsync(string resource, string action,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Role entity
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetByRankGreaterThanAsync(int rank, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, long? excludeRoleId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Resource entity
/// </summary>
public interface IResourceRepository : IRepository<Resource>
{
    Task<Resource?> GetByTypeAndNodeIdAsync(string type, long nodeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Resource>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Resource>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for RolePermission entity
/// </summary>
public interface IRolePermissionRepository : IJunctionRepository<RolePermission>
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long roleId, CancellationToken cancellationToken = default);

    Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(long permissionId,
        CancellationToken cancellationToken = default);

    Task<RolePermission?> GetByRoleAndPermissionAsync(long roleId, long permissionId,
        CancellationToken cancellationToken = default);

    Task DeleteByRoleIdAsync(long roleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserNodeRole entity
/// </summary>
public interface IUserNodeRoleRepository : IJunctionRepository<UserNodeRole>
{
    Task<IEnumerable<UserNodeRole>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserNodeRole>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserNodeRole>> GetByRoleIdAsync(long roleId, CancellationToken cancellationToken = default);

    Task<UserNodeRole?> GetByUserAndNodeAndRoleAsync(long userId, long nodeId, long roleId,
        CancellationToken cancellationToken = default);

    Task DeleteByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task DeleteByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);
}