using FAM.Domain.Authorization;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository for SigningKey management
/// </summary>
public interface ISigningKeyRepository : IRepository<SigningKey>
{
    /// <summary>
    /// Get signing key by Key ID (kid)
    /// </summary>
    Task<SigningKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the currently active signing key
    /// </summary>
    Task<SigningKey?> GetActiveKeyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all keys that can be used for verification (not revoked)
    /// </summary>
    Task<IReadOnlyList<SigningKey>> GetVerificationKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active keys (for JWKS endpoint)
    /// </summary>
    Task<IReadOnlyList<SigningKey>> GetAllActiveKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate all keys except the specified one
    /// </summary>
    Task DeactivateAllExceptAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expired keys that need cleanup
    /// </summary>
    Task<IReadOnlyList<SigningKey>> GetExpiredKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get keys expiring within the specified timespan
    /// </summary>
    Task<IReadOnlyList<SigningKey>> GetKeysExpiringWithinAsync(TimeSpan timeSpan,
        CancellationToken cancellationToken = default);
}
