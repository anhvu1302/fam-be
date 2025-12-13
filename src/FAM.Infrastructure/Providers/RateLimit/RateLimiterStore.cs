using FAM.Application.Abstractions;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.RateLimit;

/// <summary>
/// Cache-based rate limiter store that uses the configured cache provider
/// Ensures consistent rate limiting across multiple instances using Redis/cache
/// </summary>
public sealed class RateLimiterStore : IRateLimiterStore
{
    private readonly ICacheProvider _cache;
    private readonly ILogger<RateLimiterStore> _logger;

    public RateLimiterStore(ICacheProvider cache, ILogger<RateLimiterStore> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryAcquireAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetWindowKey(key, window);

            // Get current count
            var countStr = await _cache.GetAsync(cacheKey, cancellationToken);
            if (!int.TryParse(countStr, out var currentCount))
            {
                currentCount = 0;
            }

            // Check if limit exceeded
            if (currentCount >= permitLimit)
            {
                _logger.LogDebug("Rate limit exceeded for key {Key}: {Current}/{Limit}", key, currentCount, permitLimit);
                return false;
            }

            // Increment counter
            var newCount = currentCount + 1;
            await _cache.SetAsync(cacheKey, newCount.ToString(), window, cancellationToken);

            _logger.LogDebug("Rate limit check passed for key {Key}: {Current}/{Limit}", key, newCount, permitLimit);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key {Key}", key);
            // On error, allow request to prevent service disruption
            return true;
        }
    }

    public async ValueTask<int> GetCountAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get count from multiple window sizes (sliding window approach)
            var count = 0;
            var now = DateTimeOffset.UtcNow;

            // Check last 4 time windows (for potential sliding window support)
            for (int i = 0; i < 4; i++)
            {
                var windowKey = GetWindowKeyWithTimestamp(key, now.AddMinutes(-i));
                var countStr = await _cache.GetAsync(windowKey, cancellationToken);
                if (int.TryParse(countStr, out var windowCount))
                {
                    count += windowCount;
                }
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting count for key {Key}", key);
            return 0;
        }
    }

    public async ValueTask<int> GetRemainingAsync(
        string key,
        int permitLimit,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await GetCountAsync(key, cancellationToken);
            var remaining = Math.Max(0, permitLimit - count);
            return remaining;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining permits for key {Key}", key);
            return permitLimit; // Default to full limit on error
        }
    }

    public async ValueTask<int> GetRetryAfterSecondsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get TTL from cache (how long until window resets)
            var cacheKey = GetWindowKey(key, TimeSpan.FromMinutes(1));

            // Since ICacheProvider doesn't expose TTL directly, we'll use a separate TTL key
            var ttlStr = await _cache.GetAsync(GetTtlKey(cacheKey), cancellationToken);
            if (int.TryParse(ttlStr, out var ttl))
            {
                return Math.Max(1, ttl);
            }

            // Default to 60 seconds if TTL not found
            return 60;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting retry-after for key {Key}", key);
            return 60; // Default to 60 seconds on error
        }
    }

    public async ValueTask<bool> ResetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetWindowKey(key, TimeSpan.FromMinutes(1));
            await _cache.DeleteAsync(cacheKey, cancellationToken);
            await _cache.DeleteAsync(GetTtlKey(cacheKey), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for key {Key}", key);
            return false;
        }
    }

    private static string GetWindowKey(string key, TimeSpan window)
    {
        var now = DateTimeOffset.UtcNow;
        var totalSeconds = (long)now.TimeOfDay.TotalSeconds;
        var windowSeconds = (long)window.TotalSeconds;
        var windowStart = totalSeconds - (totalSeconds % windowSeconds);
        return $"ratelimit:{key}:{windowStart}";
    }

    private static string GetWindowKeyWithTimestamp(string key, DateTimeOffset timestamp)
    {
        var totalSeconds = (long)timestamp.TimeOfDay.TotalSeconds;
        var windowSeconds = 60L; // 1 minute
        var windowStart = totalSeconds - (totalSeconds % windowSeconds);
        return $"ratelimit:{key}:{windowStart}";
    }

    private static string GetTtlKey(string cacheKey) => $"{cacheKey}:ttl";
}
