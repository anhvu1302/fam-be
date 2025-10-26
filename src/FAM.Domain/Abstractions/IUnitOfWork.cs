namespace FAM.Domain.Abstractions;

/// <summary>
/// Unit of Work pattern - manages transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAssetRepository Assets { get; }
    ILocationRepository Locations { get; }
    ICompanyRepository Companies { get; }
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}