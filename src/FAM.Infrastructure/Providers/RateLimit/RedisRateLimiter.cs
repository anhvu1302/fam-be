using System.Threading.RateLimiting;

using FAM.Application.Abstractions;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.RateLimit;

/// <summary>
/// Redis-backed distributed rate limiter
/// Ensures rate limiting works consistently across multiple server instances
/// </summary>
public sealed class RedisRateLimiter : RateLimiter
{
    private readonly IRateLimiterStore _store;
    private readonly string _partitionKey;
    private readonly int _permitLimit;
    private readonly TimeSpan _window;
    private readonly ILogger _logger;
    private int _queueLimit;

    public RedisRateLimiter(
        IRateLimiterStore store,
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        int queueLimit,
        ILogger logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _partitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
        _permitLimit = permitLimit;
        _window = window;
        _queueLimit = queueLimit;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override TimeSpan? IdleDuration => null;

    public override RateLimiterStatistics? GetStatistics()
    {
        // Return null as we don't track statistics in this implementation
        // Statistics can be retrieved from Redis if needed
        return null;
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        // Use async version instead
        return new RedisRateLimitLease(false, TimeSpan.Zero);
    }

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _store.TryAcquireAsync(_partitionKey, _permitLimit, _window, cancellationToken);

            if (success) return new RedisRateLimitLease(true, TimeSpan.Zero);

            // Calculate retry after
            var remaining = await _store.GetRemainingAsync(_partitionKey, _permitLimit, cancellationToken);
            TimeSpan retryAfter = remaining <= 0 ? _window : TimeSpan.FromSeconds(1);

            return new RedisRateLimitLease(false, retryAfter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring rate limit lease for partition {Partition}", _partitionKey);
            // On error, allow request to prevent service disruption
            return new RedisRateLimitLease(true, TimeSpan.Zero);
        }
    }

    protected override void Dispose(bool disposing)
    {
        // No resources to dispose
    }

    protected override ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }

    private sealed class RedisRateLimitLease : RateLimitLease
    {
        private readonly bool _isAcquired;
        private readonly TimeSpan _retryAfter;

        public RedisRateLimitLease(bool isAcquired, TimeSpan retryAfter)
        {
            _isAcquired = isAcquired;
            _retryAfter = retryAfter;
        }

        public override bool IsAcquired => _isAcquired;

        public override IEnumerable<string> MetadataNames =>
            _isAcquired ? Array.Empty<string>() : new[] { MetadataName.RetryAfter.Name };

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (metadataName == MetadataName.RetryAfter.Name && !_isAcquired)
            {
                metadata = _retryAfter;
                return true;
            }

            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            // No resources to dispose
        }
    }
}
