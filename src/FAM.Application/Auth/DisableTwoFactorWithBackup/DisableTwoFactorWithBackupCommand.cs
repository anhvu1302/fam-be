using MediatR;

namespace FAM.Application.Auth.DisableTwoFactorWithBackup;

/// <summary>
/// Command to disable 2FA using a backup code (for account recovery when device is lost)
/// </summary>
public sealed record DisableTwoFactorWithBackupCommand : IRequest<bool>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string BackupCode { get; init; }
}
