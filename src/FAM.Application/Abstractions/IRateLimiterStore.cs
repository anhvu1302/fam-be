namespace FAM.Application.Abstractions;

/// <summary>
/// Abstraction for rate limiter storage to support multiple implementations
/// (Redis, InMemory, etc.) without changing business logic
/// </summary>
public interface IRateLimiterStore
{
    /// <summary>
    /// Attempt to allow a request based on rate limiting rules
    /// Returns true if request is allowed, false if rate limit exceeded
    /// </summary>
    ValueTask<bool> TryAcquireAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current request count for a given key
    /// </summary>
    ValueTask<int> GetCountAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining permits for a given key
    /// </summary>
    ValueTask<int> GetRemainingAsync(
        string key,
        int permitLimit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get retry-after time in seconds
    /// </summary>
    ValueTask<int> GetRetryAfterSecondsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset the counter for a key
    /// </summary>
    ValueTask<bool> ResetAsync(string key, CancellationToken cancellationToken = default);
}
