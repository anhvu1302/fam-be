namespace FAM.Domain.Abstractions;

/// <summary>
/// Service for managing temporary 2FA session tokens
/// Uses in-memory/cache storage instead of JWT for better performance and simplicity
/// </summary>
public interface ITwoFactorSessionService
{
    /// <summary>
    /// Create a temporary 2FA session token
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="expirationMinutes">Token expiration time in minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session token string</returns>
    Task<string> CreateSessionAsync(long userId, int expirationMinutes = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate and extract user ID from 2FA session token
    /// </summary>
    /// <param name="token">Session token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User ID if valid, 0 if invalid or expired</returns>
    Task<long> ValidateAndGetUserIdAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a 2FA session token (e.g., after successful verification)
    /// </summary>
    /// <param name="token">Session token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeSessionAsync(string token, CancellationToken cancellationToken = default);
}
