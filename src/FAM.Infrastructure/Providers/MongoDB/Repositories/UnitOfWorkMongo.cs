using FAM.Domain.Abstractions;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of IUnitOfWork
/// MongoDB doesn't have traditional transactions like RDBMS, so this is mostly a no-op
/// </summary>
public class UnitOfWorkMongo : IUnitOfWork
{
    private readonly MongoDbContext _context;

    public UnitOfWorkMongo(
        MongoDbContext context,
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        ISigningKeyRepository signingKeyRepository)
    {
        _context = context;
        Users = userRepository;
        UserDevices = userDeviceRepository;
        SigningKeys = signingKeyRepository;

        // MongoDB not used for authorization/email templates - these are null/not implemented
        Roles = null!;
        Permissions = null!;
        RolePermissions = null!;
        UserNodeRoles = null!;
        Resources = null!;
        OrgNodes = null!;
        EmailTemplates = null!;
    }

    public IUserRepository Users { get; }
    public IUserDeviceRepository UserDevices { get; }
    public ISigningKeyRepository SigningKeys { get; }

    // Authorization repositories - not implemented for MongoDB
    public IRoleRepository Roles { get; }
    public IPermissionRepository Permissions { get; }
    public IRolePermissionRepository RolePermissions { get; }
    public IUserNodeRoleRepository UserNodeRoles { get; }
    public IResourceRepository Resources { get; }

    // Organization repositories - not implemented for MongoDB
    public IOrgNodeRepository OrgNodes { get; }

    // Email repositories - not implemented for MongoDB
    public IEmailTemplateRepository EmailTemplates { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't have a "save changes" concept like EF
        // All operations are immediate
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB supports transactions in replica sets
        // For simplicity, we'll implement this later if needed
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Nothing to dispose for MongoDB client
    }
}