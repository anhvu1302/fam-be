using System.Text.Json;

using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.DisableTwoFactorWithBackup;

/// <summary>
/// Handler for disabling 2FA using backup code (account recovery)
/// </summary>
public sealed class DisableTwoFactorWithBackupCommandHandler : IRequestHandler<DisableTwoFactorWithBackupCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DisableTwoFactorWithBackupCommandHandler> _logger;

    public DisableTwoFactorWithBackupCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DisableTwoFactorWithBackupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DisableTwoFactorWithBackupCommand request, CancellationToken cancellationToken)
    {
        // Find user by username
        User? user = await _unitOfWork.Users.FindByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Disable 2FA with backup code failed: User not found for username: {Username}",
                request.Username);
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_CREDENTIALS, "Username or email not found");
        }

        // Verify password
        if (!user.Password.Verify(request.Password))
        {
            _logger.LogWarning("Disable 2FA with backup code failed: Invalid password for user: {UserId}", user.Id);
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_PASSWORD, "Invalid password");
        }

        // Check if 2FA is enabled
        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorBackupCodes))
        {
            _logger.LogWarning("Disable 2FA with backup code failed: 2FA not enabled for user: {UserId}", user.Id);
            throw new InvalidOperationException("Two-factor authentication is not enabled for this account");
        }

        // Deserialize stored backup codes
        List<string>? hashedBackupCodes;
        try
        {
            hashedBackupCodes = JsonSerializer.Deserialize<List<string>>(user.TwoFactorBackupCodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize backup codes for user: {UserId}", user.Id);
            throw new InvalidOperationException("Failed to verify backup code");
        }

        if (hashedBackupCodes == null || hashedBackupCodes.Count == 0)
        {
            _logger.LogWarning("No backup codes found for user: {UserId}", user.Id);
            throw new InvalidOperationException("No backup codes available");
        }

        // Verify backup code and remove it if valid
        var codeFound = false;
        string? matchedHash = null;

        foreach (var hashedCode in hashedBackupCodes)
            if (BCrypt.Net.BCrypt.Verify(request.BackupCode, hashedCode))
            {
                codeFound = true;
                matchedHash = hashedCode;
                break;
            }

        if (!codeFound)
        {
            _logger.LogWarning("Invalid backup code provided for user: {UserId}", user.Id);
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_BACKUP_CODE, "Invalid backup code");
        }

        // Remove the used backup code
        hashedBackupCodes.Remove(matchedHash!);

        // If no more backup codes left, disable 2FA completely
        // Otherwise, save the remaining codes
        if (hashedBackupCodes.Count == 0)
            user.DisableTwoFactor();
        else
            // Still have backup codes - update but keep 2FA disabled for now
            // User needs to re-enable 2FA to get security back
            user.DisableTwoFactor();

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
