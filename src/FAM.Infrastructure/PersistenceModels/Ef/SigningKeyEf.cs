using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for SigningKey
/// Stores RSA keys for JWT signing and verification
/// </summary>
[Table("signing_keys")]
public class SigningKeyEf : BaseEntityEf
{
    /// <summary>
    /// Unique Key ID (kid) used in JWT header
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// RSA Public Key in PEM format
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// RSA Private Key in PEM format (encrypted)
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Algorithm used (RS256, RS384, RS512)
    /// </summary>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Key size in bits (2048, 3072, 4096)
    /// </summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// Key usage: "sig" for signature
    /// </summary>
    public string Use { get; set; } = "sig";

    /// <summary>
    /// Key type: "RSA"
    /// </summary>
    public string KeyType { get; set; } = "RSA";

    /// <summary>
    /// Is this the currently active key for signing new tokens?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Has this key been revoked?
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// When the key was revoked
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for revocation
    /// </summary>
    public string? RevocationReason { get; set; }

    /// <summary>
    /// When the key expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// When the key was last used to sign a token
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Optional description/notes
    /// </summary>
    public string? Description { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }
}
