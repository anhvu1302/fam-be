using FAM.Domain.Common;

namespace FAM.Domain.Authorization;

/// <summary>
/// RSA Signing Key for JWT tokens
/// Supports key rotation and revocation
/// </summary>
public class SigningKey : BaseEntity
{
    /// <summary>
    /// Unique Key ID (kid) used in JWT header
    /// </summary>
    public string KeyId { get; private set; } = null!;

    /// <summary>
    /// RSA Public Key in PEM format
    /// </summary>
    public string PublicKey { get; private set; } = null!;

    /// <summary>
    /// RSA Private Key in PEM format (encrypted)
    /// </summary>
    public string PrivateKey { get; private set; } = null!;

    /// <summary>
    /// Algorithm used (RS256, RS384, RS512)
    /// </summary>
    public string Algorithm { get; private set; } = "RS256";

    /// <summary>
    /// Key size in bits (2048, 3072, 4096)
    /// </summary>
    public int KeySize { get; private set; } = 2048;

    /// <summary>
    /// Key usage: "sig" for signature
    /// </summary>
    public string Use { get; private set; } = "sig";

    /// <summary>
    /// Key type: "RSA"
    /// </summary>
    public string KeyType { get; private set; } = "RSA";

    /// <summary>
    /// Is this the currently active key for signing new tokens?
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Has this key been revoked?
    /// Revoked keys should not be used for verification
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// When the key was revoked
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Reason for revocation
    /// </summary>
    public string? RevocationReason { get; private set; }

    /// <summary>
    /// When the key expires (null = never expires)
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// When the key was last used to sign a token
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }

    /// <summary>
    /// Optional description/notes about this key
    /// </summary>
    public string? Description { get; private set; }

    // Required for EF
    private SigningKey()
    {
    }

    /// <summary>
    /// Create a new signing key
    /// </summary>
    public static SigningKey Create(
        string keyId,
        string publicKey,
        string privateKey,
        string algorithm = "RS256",
        int keySize = 2048,
        DateTime? expiresAt = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new DomainException("KeyId is required");
        if (string.IsNullOrWhiteSpace(publicKey))
            throw new DomainException("PublicKey is required");
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new DomainException("PrivateKey is required");

        return new SigningKey
        {
            KeyId = keyId,
            PublicKey = publicKey,
            PrivateKey = privateKey,
            Algorithm = algorithm,
            KeySize = keySize,
            Use = "sig",
            KeyType = "RSA",
            IsActive = true,
            IsRevoked = false,
            ExpiresAt = expiresAt,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Activate this key for signing new tokens
    /// </summary>
    public void Activate()
    {
        if (IsRevoked)
            throw new DomainException("Cannot activate a revoked key");
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow)
            throw new DomainException("Cannot activate an expired key");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate this key (will not be used for signing, but can still verify)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revoke this key (cannot be used for signing or verification)
    /// </summary>
    public void Revoke(string? reason = null)
    {
        if (IsRevoked)
            throw new DomainException("Key is already revoked");

        IsRevoked = true;
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update last used timestamp
    /// </summary>
    public void MarkAsUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if key is valid for signing
    /// </summary>
    public bool CanSign()
    {
        if (!IsActive) return false;
        if (IsRevoked) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return false;
        return true;
    }

    /// <summary>
    /// Check if key is valid for verification
    /// </summary>
    public bool CanVerify()
    {
        // Revoked keys cannot be used for verification
        if (IsRevoked) return false;
        return true;
    }

    /// <summary>
    /// Check if key is expired
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
}