namespace FAM.Application.Auth.Shared;

/// <summary>
/// Request to disable 2FA using backup code (for account recovery)
/// Validated by DisableTwoFactorWithBackupRequestValidator
/// </summary>
public sealed record DisableTwoFactorWithBackupRequest
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Password for authentication
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// One of the backup codes provided during 2FA setup
    /// Format: xxxxx-xxxxx (example: 881eb-53018)
    /// </summary>
    public required string BackupCode { get; init; }
}
