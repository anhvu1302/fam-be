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
        IUserRepository userRepository,
        IUserDeviceRepository userDeviceRepository,
        ISigningKeyRepository signingKeyRepository)
    {
        _context = context;
        Users = userRepository;
        UserDevices = userDeviceRepository;
        SigningKeys = signingKeyRepository;
    }

    public IUserRepository Users { get; }
    public IUserDeviceRepository UserDevices { get; }
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