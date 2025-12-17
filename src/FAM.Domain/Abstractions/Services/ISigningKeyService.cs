using FAM.Domain.Authorization;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Service for managing RSA signing keys for JWT
/// </summary>
public interface ISigningKeyService
{
    /// <summary>
    /// Get the currently active signing key for token generation
    /// If no active key exists, generates a new one
    /// </summary>
    Task<SigningKey> GetOrCreateActiveKeyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a new RSA signing key
    /// </summary>
    Task<SigningKey> GenerateKeyAsync(
        string algorithm = "RS256",
        int keySize = 2048,
        DateTime? expiresAt = null,
        string? description = null,
        bool activateImmediately = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a specific key (deactivates all others)
    /// </summary>
    Task ActivateKeyAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a specific key
    /// </summary>
    Task DeactivateKeyAsync(long keyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a key (cannot be used for signing or verification)
    /// </summary>
    Task RevokeKeyAsync(long keyId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotate keys - generate new active key and optionally revoke old
    /// </summary>
    Task<SigningKey> RotateKeyAsync(
        string algorithm = "RS256",
        int keySize = 2048,
        DateTime? expiresAt = null,
        string? description = null,
        bool revokeOldKey = false,
        string? revocationReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get signing key by Key ID (kid)
    /// </summary>
    Task<SigningKey?> GetKeyByKeyIdAsync(string keyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a revoked key permanently
    /// </summary>
    Task DeleteKeyAsync(long keyId, CancellationToken cancellationToken = default);
}
