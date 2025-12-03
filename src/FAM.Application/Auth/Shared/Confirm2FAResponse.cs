namespace FAM.Application.Auth.Shared;

/// <summary>
/// Response for confirming 2FA setup with backup codes
/// </summary>
public sealed record Confirm2FAResponse
{
    /// <summary>
    /// Indicates if 2FA was successfully enabled
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// List of backup codes for account recovery
    /// These codes should be stored securely by the user
    /// </summary>
    public required List<string> BackupCodes { get; init; }

    /// <summary>
    /// Message to display to user
    /// </summary>
    public string Message { get; init; } =
        "Two-factor authentication has been enabled successfully. Please save your backup codes in a secure location.";
}