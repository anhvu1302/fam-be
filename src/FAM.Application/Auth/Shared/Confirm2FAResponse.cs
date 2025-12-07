namespace FAM.Application.Auth.Shared;

/// <summary>
/// Response for confirming 2FA setup with backup codes
/// Used internally by Application handlers
/// Success/Message are handled by ApiSuccessResponse wrapper in WebApi layer
/// </summary>
public sealed record Confirm2FAResponse
{
    /// <summary>
    /// List of backup codes for account recovery
    /// These codes should be stored securely by the user
    /// </summary>
    public required List<string> BackupCodes { get; init; }
}