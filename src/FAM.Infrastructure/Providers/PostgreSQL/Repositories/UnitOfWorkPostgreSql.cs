using FAM.Domain.Abstractions;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUnitOfWork
/// </summary>
public class UnitOfWorkPostgreSql : IUnitOfWork
{
    private readonly PostgreSqlDbContext _context;

    public UnitOfWorkPostgreSql(
        PostgreSqlDbContext context,
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUserNodeRoleRepository userNodeRoleRepository,
        IResourceRepository resourceRepository,
        IOrgNodeRepository orgNodeRepository,
        ISigningKeyRepository signingKeyRepository)
    {
        _context = context;
        Users = userRepository;
        UserDevices = userDeviceRepository;
        Roles = roleRepository;
        Permissions = permissionRepository;
        RolePermissions = rolePermissionRepository;
        UserNodeRoles = userNodeRoleRepository;
        Resources = resourceRepository;
        OrgNodes = orgNodeRepository;
        SigningKeys = signingKeyRepository;
    }

    // User repositories
    public IUserRepository Users { get; }
    public IUserDeviceRepository UserDevices { get; }

    // Authorization repositories
    public IRoleRepository Roles { get; }
    public IPermissionRepository Permissions { get; }
    public IRolePermissionRepository RolePermissions { get; }
    public IUserNodeRoleRepository UserNodeRoles { get; }
    public IResourceRepository Resources { get; }

    // Organization repositories
    public IOrgNodeRepository OrgNodes { get; }

    // System repositories
    public ISigningKeyRepository SigningKeys { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}