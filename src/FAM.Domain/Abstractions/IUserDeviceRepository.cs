using FAM.Domain.Users.Entities;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface cho UserDevice aggregate
/// </summary>
public interface IUserDeviceRepository : IRepository<UserDevice>
{
    Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetActiveDevicesByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> IsDeviceIdTakenAsync(string deviceId, long? excludeDeviceId = null, CancellationToken cancellationToken = default);
    Task DeactivateDeviceAsync(long deviceId, CancellationToken cancellationToken = default);
    Task DeactivateAllUserDevicesAsync(long userId, long? excludeDeviceId = null, CancellationToken cancellationToken = default);
}