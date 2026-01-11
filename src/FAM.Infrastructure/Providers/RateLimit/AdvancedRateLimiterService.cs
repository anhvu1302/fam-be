using FAM.Domain.Abstractions.RateLimiting;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.RateLimit;

/// <summary>
/// Advanced rate limiter service for custom rate limiting scenarios
/// Supports per-user, per-endpoint, and complex rate limiting rules
/// </summary>
public sealed class AdvancedRateLimiterService
{
    private readonly IRateLimiterStore _store;
    private readonly ILogger<AdvancedRateLimiterService> _logger;

    public AdvancedRateLimiterService(
        IRateLimiterStore store,
        ILogger<AdvancedRateLimiterService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check rate limit for a specific user
    /// </summary>
    public async ValueTask<RateLimitResult> CheckUserLimitAsync(
        long userId,
        int permitLimit = 100,
        TimeSpan? window = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan actualWindow = window ?? TimeSpan.FromHours(1);
        string key = $"user:{userId}";

        bool allowed = await _store.TryAcquireAsync(
            key,
            permitLimit,
            actualWindow,
            cancellationToken);

        if (!allowed)
        {
            int remaining = await _store.GetRemainingAsync(key, permitLimit, cancellationToken);
            int retryAfter = await _store.GetRetryAfterSecondsAsync(key, cancellationToken);

            _logger.LogWarning(
                "User {UserId} rate limit exceeded. Limit: {Limit}, RetryAfter: {RetryAfter}s",
                userId, permitLimit, retryAfter);

            return RateLimitResult.Exceeded(retryAfter);
        }

        int count = await _store.GetCountAsync(key, cancellationToken);
        return RateLimitResult.Allowed(permitLimit - count);
    }

    /// <summary>
    /// Check rate limit for a specific endpoint per user
    /// Example: 10 API calls per 24 hours per user
    /// </summary>
    public async ValueTask<RateLimitResult> CheckEndpointLimitAsync(
        long userId,
        string endpoint,
        int permitLimit = 100,
        TimeSpan? window = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan actualWindow = window ?? TimeSpan.FromHours(24);
        string key = $"endpoint:{endpoint}:user:{userId}";

        bool allowed = await _store.TryAcquireAsync(
            key,
            permitLimit,
            actualWindow,
            cancellationToken);

        if (!allowed)
        {
            int remaining = await _store.GetRemainingAsync(key, permitLimit, cancellationToken);
            int retryAfter = await _store.GetRetryAfterSecondsAsync(key, cancellationToken);

            _logger.LogWarning(
                "User {UserId} exceeded limit for endpoint {Endpoint}. Limit: {Limit}, RetryAfter: {RetryAfter}s",
                userId, endpoint, permitLimit, retryAfter);

            return RateLimitResult.Exceeded(retryAfter);
        }

        int count = await _store.GetCountAsync(key, cancellationToken);
        return RateLimitResult.Allowed(permitLimit - count);
    }

    /// <summary>
    /// Check rate limit for sensitive operations (login attempts, OTP, etc.)
    /// </summary>
    public async ValueTask<RateLimitResult> CheckSensitiveOperationAsync(
        string identifier, // IP or user ID
        string operation, // "login", "otp_verify", "password_reset", etc.
        int permitLimit = 5,
        TimeSpan? window = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan actualWindow = window ?? TimeSpan.FromMinutes(15);
        string key = $"sensitive:{operation}:{identifier}";

        bool allowed = await _store.TryAcquireAsync(
            key,
            permitLimit,
            actualWindow,
            cancellationToken);

        if (!allowed)
        {
            int remaining = await _store.GetRemainingAsync(key, permitLimit, cancellationToken);
            int retryAfter = await _store.GetRetryAfterSecondsAsync(key, cancellationToken);

            _logger.LogWarning(
                "Sensitive operation {Operation} rate limit exceeded for {Identifier}. " +
                "Limit: {Limit}, RetryAfter: {RetryAfter}s",
                operation, identifier, permitLimit, retryAfter);

            return RateLimitResult.Exceeded(retryAfter);
        }

        int count = await _store.GetCountAsync(key, cancellationToken);
        return RateLimitResult.Allowed(permitLimit - count);
    }

    /// <summary>
    /// Check cumulative rate limit (multiple checks count toward same limit)
    /// Example: Both login and password reset count toward auth attempts
    /// </summary>
    public async ValueTask<RateLimitResult> CheckCumulativeLimitAsync(
        string identifier,
        string[] operations, // ["login", "forgot_password", "register"]
        int permitLimit = 20,
        TimeSpan? window = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan actualWindow = window ?? TimeSpan.FromHours(1);
        string baseKey = $"cumulative:{identifier}";

        bool allowed = await _store.TryAcquireAsync(
            baseKey,
            permitLimit,
            actualWindow,
            cancellationToken);

        if (!allowed)
        {
            int remaining = await _store.GetRemainingAsync(baseKey, permitLimit, cancellationToken);
            int retryAfter = await _store.GetRetryAfterSecondsAsync(baseKey, cancellationToken);

            _logger.LogWarning(
                "Cumulative rate limit exceeded for {Identifier} on operations: {Operations}. " +
                "Limit: {Limit}, RetryAfter: {RetryAfter}s",
                identifier, string.Join(", ", operations), permitLimit, retryAfter);

            return RateLimitResult.Exceeded(retryAfter);
        }

        int count = await _store.GetCountAsync(baseKey, cancellationToken);
        return RateLimitResult.Allowed(permitLimit - count);
    }

    /// <summary>
    /// Whitelist an identifier (bypass rate limiting)
    /// Useful for trusted IPs, premium users, etc.
    /// </summary>
    public async ValueTask<bool> WhitelistAsync(
        string identifier,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        string key = $"whitelist:{identifier}";
        // Using TryAcquireAsync internally since we only have IRateLimiterStore
        // Store whitelist with large permitLimit and short window
        bool allowed = await _store.TryAcquireAsync(key, int.MaxValue, duration, cancellationToken);
        return allowed;
    }

    /// <summary>
    /// Check if identifier is whitelisted
    /// </summary>
    public async ValueTask<bool> IsWhitelistedAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        string key = $"whitelist:{identifier}";
        int count = await _store.GetCountAsync(key, cancellationToken);
        return count > 0; // If it has a count, it was whitelisted
    }

    /// <summary>
    /// Manually reset a rate limit counter
    /// </summary>
    public async ValueTask<bool> ResetLimitAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _store.ResetAsync(key, cancellationToken);
    }

    /// <summary>
    /// Get current usage for monitoring
    /// </summary>
    public async ValueTask<RateLimitUsage> GetUsageAsync(
        string key,
        int permitLimit,
        CancellationToken cancellationToken = default)
    {
        int count = await _store.GetCountAsync(key, cancellationToken);
        int remaining = await _store.GetRemainingAsync(key, permitLimit, cancellationToken);
        int retryAfter = await _store.GetRetryAfterSecondsAsync(key, cancellationToken);

        return new RateLimitUsage
        {
            TotalLimit = permitLimit,
            CurrentCount = count,
            RemainingRequests = remaining,
            RetryAfterSeconds = retryAfter,
            IsExceeded = remaining <= 0
        };
    }
}

/// <summary>
/// Result of a rate limit check
/// </summary>
public sealed class RateLimitResult
{
    public bool IsAllowed { get; init; }
    public int RemainingRequests { get; init; }
    public int RetryAfterSeconds { get; init; }

    public static RateLimitResult Allowed(int remaining)
    {
        return new RateLimitResult
        {
            IsAllowed = true,
            RemainingRequests = remaining,
            RetryAfterSeconds = 0
        };
    }

    public static RateLimitResult Exceeded(int retryAfterSeconds)
    {
        return new RateLimitResult
        {
            IsAllowed = false,
            RemainingRequests = 0,
            RetryAfterSeconds = retryAfterSeconds
        };
    }
}

/// <summary>
/// Rate limit usage information for monitoring
/// </summary>
public sealed class RateLimitUsage
{
    public int TotalLimit { get; init; }
    public int CurrentCount { get; init; }
    public int RemainingRequests { get; init; }
    public int RetryAfterSeconds { get; init; }
    public bool IsExceeded { get; init; }

    public double UsagePercentage => TotalLimit > 0 ? CurrentCount * 100.0 / TotalLimit : 0;
}
