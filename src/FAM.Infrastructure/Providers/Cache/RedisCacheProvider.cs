using FAM.Domain.Abstractions.Cache;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace FAM.Infrastructure.Providers.Cache;

/// <summary>
/// Redis implementation of ICacheProvider
/// Wraps StackExchange.Redis to provide a consistent abstraction layer
/// </summary>
public sealed class RedisCacheProvider : ICacheProvider
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheProvider> _logger;
    private readonly IDatabase _db;

    public RedisCacheProvider(IConnectionMultiplexer redis, ILogger<RedisCacheProvider> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = _redis.GetDatabase();
    }

    public async ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            RedisValue value = await _db.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get value for key: {Key}", key);
            return null;
        }
    }

    public async ValueTask<bool> SetAsync(
        string key,
        string value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            When when = When.Always;
            bool keepTtl = false;
            return await _db.StringSetAsync(key, value, expiration, keepTtl, when);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set value for key: {Key}", key);
            return false;
        }
    }

    public async ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete key: {Key}", key);
            return false;
        }
    }

    public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of key: {Key}", key);
            return false;
        }
    }

    public async ValueTask<bool> ExpireAsync(string key, TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.KeyExpireAsync(key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set expiration for key: {Key}", key);
            return false;
        }
    }

    // List operations
    public async ValueTask<long> ListRightPushAsync(string key, string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.ListRightPushAsync(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push to list: {Key}", key);
            return 0;
        }
    }

    public async ValueTask<string?> ListLeftPopAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            RedisValue value = await _db.ListLeftPopAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pop from list: {Key}", key);
            return null;
        }
    }

    public async ValueTask<long> ListLengthAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.ListLengthAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get list length: {Key}", key);
            return 0;
        }
    }

    public async ValueTask<IReadOnlyList<string>> ListRangeAsync(
        string key,
        long start = 0,
        long stop = -1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            RedisValue[] values = await _db.ListRangeAsync(key, start, stop);
            return values.Select(v => v.ToString()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get list range: {Key}", key);
            return Array.Empty<string>();
        }
    }

    // Hash operations
    public async ValueTask<bool> HashSetAsync(
        string key,
        string field,
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.HashSetAsync(key, field, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set hash field: {Key}.{Field}", key, field);
            return false;
        }
    }

    public async ValueTask<string?> HashGetAsync(string key, string field,
        CancellationToken cancellationToken = default)
    {
        try
        {
            RedisValue value = await _db.HashGetAsync(key, field);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hash field: {Key}.{Field}", key, field);
            return null;
        }
    }

    public async ValueTask<bool> HashDeleteAsync(string key, string field,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.HashDeleteAsync(key, field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete hash field: {Key}.{Field}", key, field);
            return false;
        }
    }

    public async ValueTask<Dictionary<string, string>> HashGetAllAsync(string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HashEntry[] entries = await _db.HashGetAllAsync(key);
            return entries.ToDictionary(
                e => e.Name.ToString(),
                e => e.Value.ToString()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all hash fields: {Key}", key);
            return new Dictionary<string, string>();
        }
    }

    // Set operations
    public async ValueTask<bool> SetAddAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.SetAddAsync(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add to set: {Key}", key);
            return false;
        }
    }

    public async ValueTask<bool> SetRemoveAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.SetRemoveAsync(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove from set: {Key}", key);
            return false;
        }
    }

    public async ValueTask<bool> SetContainsAsync(string key, string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.SetContainsAsync(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check set membership: {Key}", key);
            return false;
        }
    }

    public async ValueTask<IReadOnlyList<string>> SetMembersAsync(string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            RedisValue[] values = await _db.SetMembersAsync(key);
            return values.Select(v => v.ToString()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get set members: {Key}", key);
            return Array.Empty<string>();
        }
    }
}
