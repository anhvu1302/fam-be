namespace FAM.Domain.Abstractions;

/// <summary>
/// Unit of Work pattern - manages transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // User repositories
    IUserRepository Users { get; }
    IUserDeviceRepository UserDevices { get; }

    // Authorization repositories
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    IRolePermissionRepository RolePermissions { get; }
    IUserNodeRoleRepository UserNodeRoles { get; }
    IResourceRepository Resources { get; }

    // Organization repositories
    IOrgNodeRepository OrgNodes { get; }

    // System repositories
    ISigningKeyRepository SigningKeys { get; }
    IEmailTemplateRepository EmailTemplates { get; }
    ISystemSettingRepository SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
