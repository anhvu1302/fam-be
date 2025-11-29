using System.ComponentModel.DataAnnotations;

namespace FAM.WebApi.Models.Auth;

/// <summary>
/// Request to generate a new signing key
/// </summary>
public class GenerateSigningKeyRequestModel
{
    /// <summary>
    /// Algorithm (RS256, RS384, RS512)
    /// </summary>
    [RegularExpression("^(RS256|RS384|RS512)$", ErrorMessage = "Algorithm must be RS256, RS384, or RS512")]
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Key size in bits (2048, 3072, 4096)
    /// </summary>
    [Range(2048, 4096, ErrorMessage = "Key size must be 2048, 3072, or 4096")]
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// When the key expires (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Description/notes
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Activate immediately and deactivate other keys
    /// </summary>
    public bool ActivateImmediately { get; set; } = true;
}

/// <summary>
/// Request to revoke a signing key
/// </summary>
public class RevokeSigningKeyRequestModel
{
    /// <summary>
    /// Reason for revocation
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// Request to rotate keys
/// </summary>
public class RotateKeyRequestModel
{
    /// <summary>
    /// Algorithm for new key
    /// </summary>
    [RegularExpression("^(RS256|RS384|RS512)$", ErrorMessage = "Algorithm must be RS256, RS384, or RS512")]
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// Key size for new key
    /// </summary>
    [Range(2048, 4096, ErrorMessage = "Key size must be 2048, 3072, or 4096")]
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// Expiry for new key
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Description for new key
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether to revoke the old key (vs just deactivate)
    /// </summary>
    public bool RevokeOldKey { get; set; } = false;

    /// <summary>
    /// Reason for revoking old key
    /// </summary>
    [MaxLength(500)]
    public string? RevocationReason { get; set; }
}