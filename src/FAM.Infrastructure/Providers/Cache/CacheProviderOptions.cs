namespace FAM.Infrastructure.Providers.Cache;

/// <summary>
/// Configuration options for cache providers
/// </summary>
public class CacheProviderOptions
{
    public const string SectionName = "Cache";

    /// <summary>
    /// Cache provider type: "Redis", "InMemory", "Memcached", etc.
    /// </summary>
    public string Provider { get; set; } = "Redis";

    /// <summary>
    /// Redis-specific configuration
    /// </summary>
    public RedisOptions Redis { get; set; } = new();

    /// <summary>
    /// In-memory cache configuration
    /// </summary>
    public InMemoryOptions InMemory { get; set; } = new();
}

public class RedisOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public int Database { get; set; } = 0;
    public bool AbortOnConnectFail { get; set; } = false;
    public int ConnectRetry { get; set; } = 3;
    public int ConnectTimeout { get; set; } = 5000;
    public bool UseSsl { get; set; } = false;
}

public class InMemoryOptions
{
    /// <summary>
    /// Maximum number of items to store in cache (0 = unlimited)
    /// </summary>
    public int MaxItems { get; set; } = 10000;

    /// <summary>
    /// Default expiration time in minutes (0 = no expiration)
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 60;
}
