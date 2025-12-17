using FAM.Domain.Authorization;

namespace FAM.WebApi.Contracts.Auth;

#region Response Records

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

#endregion

#region Request Records

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
/// Request to rotate signing keys
/// </summary>
public class RotateKeyRequest
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
    /// Revoke all old keys
    /// </summary>
    public bool RevokeOldKey { get; set; } = true;

    /// <summary>
    /// Reason for revocation
    /// </summary>
    public string? RevocationReason { get; set; }
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

#endregion

#region Mapper Extension Methods

/// <summary>
/// Extension methods for mapping SigningKey domain entities to contracts
/// </summary>
public static class SigningKeyContractMappers
{
    /// <summary>
    /// Convert SigningKey entity to SigningKeyResponse contract
    /// </summary>
    public static SigningKeyResponse ToSigningKeyResponse(this SigningKey key)
    {
        return new SigningKeyResponse
        {
            Id = key.Id,
            KeyId = key.KeyId,
            Algorithm = key.Algorithm,
            KeySize = key.KeySize,
            IsActive = key.IsActive,
            IsRevoked = key.IsRevoked,
            RevokedAt = key.RevokedAt,
            RevocationReason = key.RevocationReason,
            ExpiresAt = key.ExpiresAt,
            LastUsedAt = key.LastUsedAt,
            Description = key.Description,
            CreatedAt = key.CreatedAt,
            IsExpired = key.IsExpired(),
            CanSign = !key.IsRevoked && !key.IsExpired() && key.IsActive,
            CanVerify = !key.IsRevoked && !key.IsExpired()
        };
    }

    /// <summary>
    /// Convert collection of SigningKey entities to SigningKeyResponse contracts
    /// </summary>
    public static IEnumerable<SigningKeyResponse> ToSigningKeyResponses(this IEnumerable<SigningKey> keys)
    {
        return keys.Select(k => k.ToSigningKeyResponse());
    }
}

#endregion
