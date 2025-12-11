namespace FAM.Application.Auth.Services;

/// <summary>
/// Service for managing blacklisted JWT tokens
/// Used to invalidate tokens before their natural expiration
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Add a token to the blacklist (invalidate it)
    /// </summary>
    /// <param name="token">The JWT access token to blacklist</param>
    /// <param name="expiryTime">When the token would naturally expire (TTL for cache)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BlacklistTokenAsync(string token, DateTime expiryTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a token is blacklisted
    /// </summary>
    /// <param name="token">The JWT access token to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is blacklisted (invalidated)</returns>
    Task<bool> IsTokenBlacklistedAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Blacklist all tokens for a specific user ID
    /// Used when logging out from all devices
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BlacklistUserTokensAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if all tokens for a user are blacklisted
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> AreUserTokensBlacklistedAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Blacklist a token by its JTI (JWT ID)
    /// Used for immediate token revocation when session is deleted
    /// </summary>
    /// <param name="jti">JWT ID from the token claims</param>
    /// <param name="expiryTime">When the token would naturally expire (TTL for cache)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BlacklistTokenByJtiAsync(string jti, DateTime expiryTime, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a token is blacklisted by its JTI
    /// </summary>
    /// <param name="jti">JWT ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsTokenBlacklistedByJtiAsync(string jti, CancellationToken cancellationToken = default);
}