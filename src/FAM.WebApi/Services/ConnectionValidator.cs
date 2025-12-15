using Minio;
using Minio.DataModel.Result;

using MongoDB.Bson;
using MongoDB.Driver;

using Npgsql;

using StackExchange.Redis;

namespace FAM.WebApi.Services;

/// <summary>
/// Validates all external connections at application startup based on configured providers.
/// Ensures Database (PostgreSQL/MongoDB), Cache (Redis/InMemory), and Storage (MinIO) are accessible.
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
        _logger.LogInformation("Starting connection validation based on configured providers...");

        var errors = new List<string>();

        // Get configured providers
        var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "PostgreSQL";
        var cacheProvider = Environment.GetEnvironmentVariable("CACHE_PROVIDER") ?? "Redis";

        // Validate Database based on provider
        try
        {
            await ValidateDatabaseAsync(dbProvider, cancellationToken);
            _logger.LogInformation("✓ Database ({Provider}) connection validated", dbProvider);
        }
        catch (Exception ex)
        {
            var error = $"Database ({dbProvider}) connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        // Validate Cache based on provider
        try
        {
            await ValidateCacheAsync(cacheProvider, cancellationToken);
            _logger.LogInformation("✓ Cache ({Provider}) connection validated", cacheProvider);
        }
        catch (Exception ex)
        {
            var error = $"Cache ({cacheProvider}) connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        // Validate MinIO (always required)
        try
        {
            await ValidateMinioAsync(cancellationToken);
            _logger.LogInformation("✓ Storage (MinIO) connection validated");
        }
        catch (Exception ex)
        {
            var error = $"Storage (MinIO) connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        if (errors.Any())
        {
            var errorMessage = string.Join("\n", errors);
            _logger.LogCritical("Connection validation failed:\n{Errors}", errorMessage);
            throw new InvalidOperationException($"Connection validation failed:\n{errorMessage}");
        }

        _logger.LogInformation("✅ All connections validated successfully");
    }

    /// <summary>
    /// Validates database connection based on configured provider
    /// </summary>
    private async Task ValidateDatabaseAsync(string provider, CancellationToken cancellationToken)
    {
        switch (provider.ToLowerInvariant())
        {
            case "postgresql":
                await ValidatePostgreSqlAsync(cancellationToken);
                break;

            case "mongodb":
                await ValidateMongoDbAsync(cancellationToken);
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid DB_PROVIDER: '{provider}'. Valid values are: 'PostgreSQL', 'MongoDB'");
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
        _logger.LogDebug("Validating PostgreSQL connection...");

        // Build connection string from environment variables
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DB_NAME");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
            throw new InvalidOperationException(
                "PostgreSQL environment variables not configured. Required: DB_USER, DB_PASSWORD, DB_NAME");

        var connectionString =
            $"Host={host};Port={port};Username={username};Password={password};Database={database};Timeout=5;";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Execute simple query to verify connection and database access
        await using var cmd = new NpgsqlCommand("SELECT version()", connection);
        var version = await cmd.ExecuteScalarAsync(cancellationToken);

        if (version == null)
            throw new InvalidOperationException("PostgreSQL health check query failed");

        _logger.LogDebug("PostgreSQL version: {Version}", version.ToString()?.Split('\n')[0]);
    }

    /// <summary>
    /// Validates MongoDB database connection
    /// </summary>
    private async Task ValidateMongoDbAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Validating MongoDB connection...");

        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "27017";
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DB_NAME");

        if (string.IsNullOrEmpty(database))
            throw new InvalidOperationException("MongoDB environment variable DB_NAME is required");

        // Build connection string
        string connectionString;
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            var escapedUser = Uri.EscapeDataString(username);
            var escapedPassword = Uri.EscapeDataString(password);
            connectionString =
                $"mongodb://{escapedUser}:{escapedPassword}@{host}:{port}/?connectTimeoutMS=5000&serverSelectionTimeoutMS=5000";
        }
        else
        {
            connectionString = $"mongodb://{host}:{port}/?connectTimeoutMS=5000&serverSelectionTimeoutMS=5000";
        }

        var client = new MongoClient(connectionString);
        IMongoDatabase? db = client.GetDatabase(database);

        // Ping to verify connection
        var pingCommand = new BsonDocument("ping", 1);
        await db.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);

        _logger.LogDebug("MongoDB connection successful to database: {Database}", database);
    }

    /// <summary>
    /// Validates Redis cache connection
    /// </summary>
    private async Task ValidateRedisAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Validating Redis connection...");

        var host = Environment.GetEnvironmentVariable("REDIS_HOST");
        var portStr = Environment.GetEnvironmentVariable("REDIS_PORT");
        var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        if (string.IsNullOrEmpty(host))
            throw new InvalidOperationException("REDIS_HOST environment variable is not configured");

        if (!int.TryParse(portStr ?? "6379", out var port))
            throw new InvalidOperationException("REDIS_PORT must be a valid integer");

        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{host}:{port}" },
            AbortOnConnectFail = false,
            ConnectRetry = 2,
            ConnectTimeout = 5000,
            SyncTimeout = 5000
        };

        if (!string.IsNullOrEmpty(password))
            configOptions.Password = password;

        ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configOptions);

        if (!redis.IsConnected)
            throw new InvalidOperationException("Redis connection failed: not connected");

        // Verify we can execute commands
        IDatabase db = redis.GetDatabase();
        TimeSpan pingResult = await db.PingAsync();

        if (pingResult == TimeSpan.Zero)
            throw new InvalidOperationException("Redis health check ping failed");

        // Test basic GET/SET operations to ensure Redis is functional
        var testKey = $"_health_check_{Guid.NewGuid()}";
        var testValue = DateTime.UtcNow.ToString("O");

        var setResult = await db.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
        if (!setResult)
            throw new InvalidOperationException("Redis health check: SET operation failed");

        string? getValue = await db.StringGetAsync(testKey);
        if (getValue != testValue)
            throw new InvalidOperationException("Redis health check: GET operation failed or returned incorrect value");

        await db.KeyDeleteAsync(testKey);

        _logger.LogDebug("Redis connection validated successfully. Ping: {Ping}ms", pingResult.TotalMilliseconds);

        await redis.CloseAsync();
    }

    /// <summary>
    /// Validates MinIO storage connection
    /// </summary>
    private async Task ValidateMinioAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Validating MinIO connection...");

        var host = Environment.GetEnvironmentVariable("MINIO_HOST");
        var portStr = Environment.GetEnvironmentVariable("MINIO_PORT");
        var accessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        var secretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");
        var useSslStr = Environment.GetEnvironmentVariable("MINIO_USE_SSL");

        if (string.IsNullOrEmpty(host))
            throw new InvalidOperationException("MINIO_HOST environment variable is not configured");
        if (string.IsNullOrEmpty(portStr))
            throw new InvalidOperationException("MINIO_PORT environment variable is not configured");
        if (string.IsNullOrEmpty(accessKey))
            throw new InvalidOperationException("MINIO_ACCESS_KEY environment variable is not configured");
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("MINIO_SECRET_KEY environment variable is not configured");

        var useSSL = useSslStr != null && bool.Parse(useSslStr);
        var endpoint = $"{host}:{portStr}";

        IMinioClient? minio = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithTimeout(5000); // 5 second timeout

        if (useSSL) minio = minio.WithSSL();

        IMinioClient? client = minio.Build();

        // Try to list buckets to verify connection and credentials
        ListAllMyBucketsResult? buckets = await client.ListBucketsAsync(cancellationToken);

        if (buckets == null)
            throw new InvalidOperationException("MinIO health check failed: cannot list buckets");

        _logger.LogDebug("MinIO connection successful. Buckets found: {Count}", buckets.Buckets.Count);
    }
}
