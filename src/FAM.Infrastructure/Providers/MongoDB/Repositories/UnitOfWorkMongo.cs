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
        ICompanyRepository companyRepository,
        IUserRepository userRepository
        /*IAssetRepository assetRepository,
        ILocationRepository locationRepository*/)
    {
        _context = context;
        Companies = companyRepository;
        Users = userRepository;
        /*Assets = assetRepository;
        Locations = locationRepository;*/
    }

    public IAssetRepository Assets => throw new NotImplementedException("Asset repository not implemented yet");
    public ILocationRepository Locations => throw new NotImplementedException("Location repository not implemented yet");
    public ICompanyRepository Companies { get; }
    public IUserRepository Users { get; }

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