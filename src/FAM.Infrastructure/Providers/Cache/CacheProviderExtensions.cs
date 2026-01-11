using FAM.Application.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace FAM.Infrastructure.Providers.Cache;

/// <summary>
/// Extension methods for registering cache providers
/// </summary>
public static class CacheProviderExtensions
{
    /// <summary>
    /// Add cache provider services based on configuration
    /// Can be configured via appsettings.json "Cache" section or environment variables:
    /// - CACHE_PROVIDER: "Redis", "InMemory"
    /// - REDIS_HOST, REDIS_PORT, REDIS_PASSWORD, REDIS_DATABASE
    /// </summary>
    public static IServiceCollection AddCacheProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration with environment variable overrides
        services.Configure<CacheProviderOptions>(options =>
        {
            // Bind from appsettings first
            configuration.GetSection(CacheProviderOptions.SectionName).Bind(options);

            // Override with environment variables if present
            string? envProvider = Environment.GetEnvironmentVariable("CACHE_PROVIDER");
            if (!string.IsNullOrEmpty(envProvider))
            {
                options.Provider = envProvider;
            }

            // Redis settings
            string? redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
            if (!string.IsNullOrEmpty(redisHost))
            {
                options.Redis.Host = redisHost;
            }

            string? redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
            if (!string.IsNullOrEmpty(redisPort) && int.TryParse(redisPort, out int port))
            {
                options.Redis.Port = port;
            }

            string? redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
            if (!string.IsNullOrEmpty(redisPassword))
            {
                options.Redis.Password = redisPassword;
            }

            string? redisDatabase = Environment.GetEnvironmentVariable("REDIS_DATABASE");
            if (!string.IsNullOrEmpty(redisDatabase) && int.TryParse(redisDatabase, out int db))
            {
                options.Redis.Database = db;
            }
        });

        // Register cache provider
        services.AddSingleton<ICacheProvider>(sp =>
        {
            string providerName = Environment.GetEnvironmentVariable("CACHE_PROVIDER")
                                  ?? configuration.GetValue<string>($"{CacheProviderOptions.SectionName}:Provider")
                                  ?? "Redis";

            // Validate provider
            string[] validProviders = new[] { "redis", "inmemory", "memory" };
            if (!validProviders.Contains(providerName.ToLowerInvariant()))
            {
                throw new InvalidOperationException(
                    $"Invalid CACHE_PROVIDER value: '{providerName}'. " +
                    $"Valid values are: 'Redis', 'InMemory'. " +
                    $"Please set CACHE_PROVIDER environment variable or configure Cache:Provider in appsettings.json");
            }

            switch (providerName.ToLowerInvariant())
            {
                case "redis":
                    return CreateRedisCacheProvider(sp, configuration);

                case "inmemory":
                case "memory":
                    ILogger<InMemoryCacheProvider> logger = sp.GetRequiredService<ILogger<InMemoryCacheProvider>>();
                    logger.LogInformation("Using in-memory cache provider");
                    return new InMemoryCacheProvider(logger);

                default:
                    throw new InvalidOperationException($"Unsupported cache provider: {providerName}");
            }
        });

        return services;
    }

    private static ICacheProvider CreateRedisCacheProvider(
        IServiceProvider sp,
        IConfiguration configuration)
    {
        ILogger<RedisCacheProvider> logger = sp.GetRequiredService<ILogger<RedisCacheProvider>>();

        try
        {
            string redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";

            string redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

            string? redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? null;

            ConfigurationOptions configOptions = new()
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000
            };

            if (!string.IsNullOrEmpty(redisPassword))
            {
                configOptions.Password = redisPassword;
            }

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(configOptions);

            return new RedisCacheProvider(connection, logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to connect to Redis. Falling back to in-memory cache.");
            ILogger<InMemoryCacheProvider> memLogger = sp.GetRequiredService<ILogger<InMemoryCacheProvider>>();
            return new InMemoryCacheProvider(memLogger);
        }
    }

    /// <summary>
    /// Add Redis cache provider explicitly
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        string host = "localhost",
        int port = 6379,
        string? password = null)
    {
        services.AddSingleton<ICacheProvider>(sp =>
        {
            ILogger<RedisCacheProvider> logger = sp.GetRequiredService<ILogger<RedisCacheProvider>>();

            ConfigurationOptions configOptions = new()
            {
                EndPoints = { $"{host}:{port}" },
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000
            };

            if (!string.IsNullOrEmpty(password))
            {
                configOptions.Password = password;
            }

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(configOptions);

            return new RedisCacheProvider(connection, logger);
        });

        return services;
    }

    /// <summary>
    /// Add in-memory cache provider explicitly
    /// </summary>
    public static IServiceCollection AddInMemoryCache(this IServiceCollection services)
    {
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
        return services;
    }
}
