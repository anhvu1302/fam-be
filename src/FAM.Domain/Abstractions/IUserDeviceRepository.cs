using FAM.Domain.Users.Entities;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface cho UserDevice aggregate (uses GUID as primary key)
/// </summary>
public interface IUserDeviceRepository : IRepository<UserDevice, Guid>
{
    Task<UserDevice?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetActiveDevicesByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> IsDeviceIdTakenAsync(string deviceId, Guid? excludeDeviceId = null, CancellationToken cancellationToken = default);
    Task DeactivateDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deactivate all devices for a user (used for logout all)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="excludeDeviceId">Device ID (fingerprint string) to keep active - usually current device</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateAllUserDevicesAsync(long userId, string? excludeDeviceId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Find device by refresh token - for token refresh
    /// </summary>
    Task<UserDevice?> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}