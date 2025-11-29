namespace FAM.Application.Auth.DTOs;

/// <summary>
/// JSON Web Key representation
/// </summary>
public class JwkDto
{
    /// <summary>
    /// Key type (RSA)
    /// </summary>
    public string Kty { get; set; } = "RSA";

    /// <summary>
    /// Key use (sig = signature)
    /// </summary>
    public string Use { get; set; } = "sig";

    /// <summary>
    /// Key ID
    /// </summary>
    public string Kid { get; set; } = null!;

    /// <summary>
    /// Algorithm (RS256, RS384, RS512)
    /// </summary>
    public string Alg { get; set; } = "RS256";

    /// <summary>
    /// RSA modulus (Base64Url encoded)
    /// </summary>
    public string N { get; set; } = null!;

    /// <summary>
    /// RSA public exponent (Base64Url encoded)
    /// </summary>
    public string E { get; set; } = null!;
}

/// <summary>
/// JSON Web Key Set
/// </summary>
public class JwksDto
{
    /// <summary>
    /// Array of JWK keys
    /// </summary>
    public List<JwkDto> Keys { get; set; } = new();
}

/// <summary>
/// Response for signing key details (admin)
/// </summary>
public class SigningKeyResponse
{
    public long Id { get; set; }
    public string KeyId { get; set; } = null!;
    public string Algorithm { get; set; } = null!;
    public int KeySize { get; set; }
    public bool IsActive { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsExpired { get; set; }
    public bool CanSign { get; set; }
    public bool CanVerify { get; set; }
}

/// <summary>
/// Request to generate a new signing key
/// </summary>
public class GenerateSigningKeyRequest
{
    /// <summary>
    /// Algorithm (RS256, RS384, RS512)
    /// </summary>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Key size in bits (2048, 3072, 4096)
    /// </summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// When the key expires (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Description/notes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Activate immediately and deactivate other keys
    /// </summary>
    public bool ActivateImmediately { get; set; } = true;
}

/// <summary>
/// Request to revoke a signing key
/// </summary>
public class RevokeSigningKeyRequest
{
    /// <summary>
    /// Reason for revocation
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request to rotate keys (generate new and deactivate old)
/// </summary>
public class RotateKeyRequest
{
    /// <summary>
    /// Algorithm for new key
    /// </summary>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Key size for new key
    /// </summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// Expiry for new key
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Description for new key
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether to revoke the old key (vs just deactivate)
    /// </summary>
    public bool RevokeOldKey { get; set; } = false;

    /// <summary>
    /// Reason for revoking old key
    /// </summary>
    public string? RevocationReason { get; set; }
}