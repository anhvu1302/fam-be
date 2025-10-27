using FAM.Domain.Abstractions;
using FAM.Infrastructure.Providers.MongoDB;

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
        IUserDeviceRepository userDeviceRepository)
    {
        _context = context;
        Users = userRepository;
        UserDevices = userDeviceRepository;
    }

    public IUserRepository Users { get; }
    public IUserDeviceRepository UserDevices { get; }

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