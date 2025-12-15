using FAM.Application.Abstractions;
using FAM.Application.Auth.Services;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Auth;

/// <summary>
/// Implementation of 2FA session service using configurable cache provider
/// Supports multiple cache implementations (Redis, In-Memory) via ICacheProvider abstraction
/// Sessions persist across application restarts when using distributed cache (Redis)
/// </summary>
public class TwoFactorSessionService : ITwoFactorSessionService
{
    private readonly ICacheProvider _cache;
    private readonly ILogger<TwoFactorSessionService> _logger;
    private const string CacheKeyPrefix = "fam:2fa_session:";

    public TwoFactorSessionService(ICacheProvider cache, ILogger<TwoFactorSessionService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        // Store in cache with expiration (Redis for persistence, In-Memory for development)
        var expiration = TimeSpan.FromMinutes(expirationMinutes);
        await _cache.SetAsync(cacheKey, userId.ToString(), expiration, cancellationToken);

        _logger.LogInformation("Created 2FA session token for user {UserId} with {ExpirationMinutes} minute expiration",
            userId, expirationMinutes);

        return token;
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
        var userIdStr = await _cache.GetAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out var userId))
        {
            _logger.LogInformation("Validated 2FA session token for user {UserId}", userId);
            return userId;
        }

        _logger.LogWarning("Invalid or expired 2FA session token");
        return 0L;
    }

    /// <summary>
    /// Revoke a 2FA session token (e.g., after successful verification)
    /// </summary>
    public async Task RevokeSessionAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var cacheKey = $"{CacheKeyPrefix}{token}";
        await _cache.DeleteAsync(cacheKey, cancellationToken);
        _logger.LogInformation("Revoked 2FA session token");
    }
}
