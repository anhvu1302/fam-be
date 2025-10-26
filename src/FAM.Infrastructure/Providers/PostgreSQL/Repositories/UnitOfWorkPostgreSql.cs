using FAM.Domain.Abstractions;
using FAM.Infrastructure.Providers.PostgreSQL;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUnitOfWork
/// </summary>
public class UnitOfWorkPostgreSql : IUnitOfWork
{
    private readonly PostgreSqlDbContext _context;

    public UnitOfWorkPostgreSql(
        PostgreSqlDbContext context,
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

    /*public IAssetRepository Assets { get; }
    public ILocationRepository Locations { get; }*/
    public IAssetRepository Assets => throw new NotImplementedException("Asset repository not implemented yet");
    public ILocationRepository Locations => throw new NotImplementedException("Location repository not implemented yet");
    public ICompanyRepository Companies { get; }
    public IUserRepository Users { get; }

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