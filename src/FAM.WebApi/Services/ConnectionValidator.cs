using Minio;
using Minio.DataModel.Result;

using Npgsql;

using StackExchange.Redis;

namespace FAM.WebApi.Services;

/// <summary>
/// Validates all external connections at application startup based on configured providers.
/// Ensures Database (PostgreSQL), Cache (Redis/InMemory), and Storage (MinIO) are accessible.
/// </summary>
public class ConnectionValidator : IConnectionValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConnectionValidator> _logger;

    public ConnectionValidator(IConfiguration configuration, ILogger<ConnectionValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ValidateAllAsync(CancellationToken cancellationToken = default)
    {
        List<string> errors = new();

        // Get configured providers - DB_PROVIDER is REQUIRED
        string? dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER");
        string cacheProvider = Environment.GetEnvironmentVariable("CACHE_PROVIDER") ?? "Redis";

        // Validate Database based on provider
        try
        {
            await ValidateDatabaseAsync(dbProvider, cancellationToken);
        }
        catch (Exception ex)
        {
            string error = $"Database ({dbProvider}) connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        // Validate Cache based on provider
        try
        {
            await ValidateCacheAsync(cacheProvider, cancellationToken);
        }
        catch (Exception ex)
        {
            string error = $"Cache ({cacheProvider}) connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        // Validate MinIO (always required)
        try
        {
            await ValidateMinioAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            string error = $"Storage (MinIO) connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        if (errors.Any())
        {
            string errorMessage = string.Join("\n", errors);
            _logger.LogCritical("Connection validation failed:\n{Errors}", errorMessage);
            throw new InvalidOperationException($"Connection validation failed:\n{errorMessage}");
        }

        _logger.LogInformation("✅ All connections validated successfully");
    }

    /// <summary>
    /// Validates database connection based on configured provider
    /// </summary>
    private async Task ValidateDatabaseAsync(string? provider, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(provider))
        {
            throw new InvalidOperationException(
                "DB_PROVIDER environment variable is required. Valid value: 'PostgreSQL'");
        }

        switch (provider.ToLowerInvariant())
        {
            case "postgresql":
                await ValidatePostgreSqlAsync(cancellationToken);
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid DB_PROVIDER: '{provider}'. Valid value: 'PostgreSQL'");
        }
    }

    /// <summary>
    /// Validates cache connection based on configured provider
    /// </summary>
    private async Task ValidateCacheAsync(string provider, CancellationToken cancellationToken)
    {
        switch (provider.ToLowerInvariant())
        {
            case "redis":
                await ValidateRedisAsync(cancellationToken);
                break;

            case "inmemory":
            case "memory":
                // InMemory cache doesn't need validation - always available
                _logger.LogInformation("Cache provider is InMemory - no connection validation needed");
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid CACHE_PROVIDER: '{provider}'. Valid values are: 'Redis', 'InMemory'");
        }
    }

    /// <summary>
    /// Validates PostgreSQL database connection
    /// </summary>
    private async Task ValidatePostgreSqlAsync(CancellationToken cancellationToken)
    {
        string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        string? username = Environment.GetEnvironmentVariable("DB_USER");
        string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        string? database = Environment.GetEnvironmentVariable("DB_NAME");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
        {
            throw new InvalidOperationException(
                "PostgreSQL environment variables not configured. Required: DB_USER, DB_PASSWORD, DB_NAME");
        }

        string connectionString =
            $"Host={host};Port={port};Username={username};Password={password};Database={database};Timeout=5;";

        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Execute simple query to verify connection and database access
        await using NpgsqlCommand cmd = new("SELECT version()", connection);
        object? version = await cmd.ExecuteScalarAsync(cancellationToken);

        if (version == null)
        {
            throw new InvalidOperationException("PostgreSQL health check query failed");
        }
    }

    /// <summary>
    /// Validates Redis cache connection
    /// </summary>
    private async Task ValidateRedisAsync(CancellationToken cancellationToken)
    {
        string? host = Environment.GetEnvironmentVariable("REDIS_HOST");
        string? portStr = Environment.GetEnvironmentVariable("REDIS_PORT");
        string? password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        if (string.IsNullOrEmpty(host))
        {
            throw new InvalidOperationException("REDIS_HOST environment variable is not configured");
        }

        if (!int.TryParse(portStr ?? "6379", out int port))
        {
            throw new InvalidOperationException("REDIS_PORT must be a valid integer");
        }

        ConfigurationOptions configOptions = new()
        {
            EndPoints = { $"{host}:{port}" },
            AbortOnConnectFail = false,
            ConnectRetry = 2,
            ConnectTimeout = 5000,
            SyncTimeout = 5000
        };

        if (!string.IsNullOrEmpty(password))
        {
            configOptions.Password = password;
        }

        ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configOptions);

        if (!redis.IsConnected)
        {
            throw new InvalidOperationException("Redis connection failed: not connected");
        }

        // Verify we can execute commands
        IDatabase db = redis.GetDatabase();
        TimeSpan pingResult = await db.PingAsync();

        if (pingResult == TimeSpan.Zero)
        {
            throw new InvalidOperationException("Redis health check ping failed");
        }

        // Test basic GET/SET operations to ensure Redis is functional
        string testKey = $"_health_check_{Guid.NewGuid()}";
        string testValue = DateTime.UtcNow.ToString("O");

        bool setResult = await db.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
        if (!setResult)
        {
            throw new InvalidOperationException("Redis health check: SET operation failed");
        }

        string? getValue = await db.StringGetAsync(testKey);
        if (getValue != testValue)
        {
            throw new InvalidOperationException("Redis health check: GET operation failed or returned incorrect value");
        }

        await db.KeyDeleteAsync(testKey);

        await redis.CloseAsync();
    }

    /// <summary>
    /// Validates MinIO storage connection
    /// </summary>
    private async Task ValidateMinioAsync(CancellationToken cancellationToken)
    {
        string? host = Environment.GetEnvironmentVariable("MINIO_HOST");
        string? portStr = Environment.GetEnvironmentVariable("MINIO_PORT");
        string? accessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        string? secretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");
        string? useSslStr = Environment.GetEnvironmentVariable("MINIO_USE_SSL");

        if (string.IsNullOrEmpty(host))
        {
            throw new InvalidOperationException("MINIO_HOST environment variable is not configured");
        }

        if (string.IsNullOrEmpty(portStr))
        {
            throw new InvalidOperationException("MINIO_PORT environment variable is not configured");
        }

        if (string.IsNullOrEmpty(accessKey))
        {
            throw new InvalidOperationException("MINIO_ACCESS_KEY environment variable is not configured");
        }

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("MINIO_SECRET_KEY environment variable is not configured");
        }

        bool useSSL = useSslStr != null && bool.Parse(useSslStr);
        string endpoint = $"{host}:{portStr}";

        IMinioClient? minio = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithTimeout(5000); // 5 second timeout

        if (useSSL)
        {
            minio = minio.WithSSL();
        }

        IMinioClient? client = minio.Build();

        // Try to list buckets to verify connection and credentials
        ListAllMyBucketsResult? buckets = await client.ListBucketsAsync(cancellationToken);

        if (buckets == null)
        {
            throw new InvalidOperationException("MinIO health check failed: cannot list buckets");
        }
    }
}
