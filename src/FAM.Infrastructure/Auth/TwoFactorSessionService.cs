using FAM.Application.Auth.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Auth;

/// <summary>
/// Implementation of 2FA session service using in-memory cache
/// Can be easily replaced with Redis for distributed caching
/// </summary>
public class TwoFactorSessionService : ITwoFactorSessionService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TwoFactorSessionService> _logger;
    private const string CacheKeyPrefix = "2fa_session_";

    public TwoFactorSessionService(IMemoryCache cache, ILogger<TwoFactorSessionService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Create a temporary 2FA session token
    /// </summary>
    public async Task<string> CreateSessionAsync(long userId, int expirationMinutes = 10,
        CancellationToken cancellationToken = default)
    {
        // Generate a random session token (simpler than JWT for single-use tokens)
        var token = Guid.NewGuid().ToString("N");
        var cacheKey = $"{CacheKeyPrefix}{token}";

        // Store in cache with expiration
        MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationMinutes));

        _cache.Set(cacheKey, userId, cacheOptions);
        _logger.LogInformation("Created 2FA session token for user {UserId} with {ExpirationMinutes} minute expiration",
            userId, expirationMinutes);

        return await Task.FromResult(token);
    }

    /// <summary>
    /// Validate and extract user ID from 2FA session token
    /// </summary>
    public async Task<long> ValidateAndGetUserIdAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Attempted to validate empty 2FA session token");
            return 0;
        }

        var cacheKey = $"{CacheKeyPrefix}{token}";

        if (_cache.TryGetValue(cacheKey, out long userId))
        {
            _logger.LogInformation("Validated 2FA session token for user {UserId}", userId);
            return await Task.FromResult(userId);
        }

        _logger.LogWarning("Invalid or expired 2FA session token");
        return await Task.FromResult(0L);
    }

    /// <summary>
    /// Revoke a 2FA session token (e.g., after successful verification)
    /// </summary>
    public async Task RevokeSessionAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var cacheKey = $"{CacheKeyPrefix}{token}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Revoked 2FA session token");

        await Task.CompletedTask;
    }
}
