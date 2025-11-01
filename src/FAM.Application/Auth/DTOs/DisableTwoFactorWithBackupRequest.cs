using System.ComponentModel.DataAnnotations;

namespace FAM.Application.Auth.DTOs;

/// <summary>
/// Request to disable 2FA using backup code (for account recovery)
/// </summary>
public sealed record DisableTwoFactorWithBackupRequest
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    [Required]
    public required string Username { get; init; }
    
    /// <summary>
    /// Password for authentication
    /// </summary>
    [Required]
    public required string Password { get; init; }
    
    /// <summary>
    /// One of the backup codes provided during 2FA setup
    /// Format: xxxxx-xxxxx (example: 881eb-53018)
    /// </summary>
    [Required]
    public required string BackupCode { get; init; }
}
