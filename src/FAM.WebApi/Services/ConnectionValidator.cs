using System.Text;
using Minio;
using Npgsql;
using StackExchange.Redis;

namespace FAM.WebApi.Services;

/// <summary>
/// Validates all external connections at application startup.
/// Ensures PostgreSQL, Redis, and MinIO are accessible before the application starts.
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
        _logger.LogInformation("Starting connection validation...");

        var errors = new List<string>();

        // Validate PostgreSQL
        try
        {
            await ValidatePostgreSqlAsync(cancellationToken);
            _logger.LogInformation("✓ PostgreSQL connection validated");
        }
        catch (Exception ex)
        {
            var error = $"PostgreSQL connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        // Validate Redis
        try
        {
            await ValidateRedisAsync(cancellationToken);
            _logger.LogInformation("✓ Redis connection validated");
        }
        catch (Exception ex)
        {
            var error = $"Redis connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        // Validate MinIO
        try
        {
            await ValidateMinioAsync(cancellationToken);
            _logger.LogInformation("✓ MinIO connection validated");
        }
        catch (Exception ex)
        {
            var error = $"MinIO connection failed: {ex.Message}";
            _logger.LogError(ex, error);
            errors.Add(error);
        }

        if (errors.Any())
        {
            var errorMessage = string.Join("\n", errors);
            _logger.LogCritical("Connection validation failed:\n{Errors}", errorMessage);
            throw new InvalidOperationException($"Connection validation failed:\n{errorMessage}");
        }

        _logger.LogInformation("All connections validated successfully");
    }

    private async Task ValidatePostgreSqlAsync(CancellationToken cancellationToken)
    {
        // Build connection string from environment variables
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DB_NAME");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
            throw new InvalidOperationException(
                "PostgreSQL environment variables not configured (DB_USER, DB_PASSWORD, DB_NAME required)");

        var connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Execute simple query to verify connection
        await using var cmd = new NpgsqlCommand("SELECT 1", connection);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);

        if (result == null || !result.Equals(1))
            throw new InvalidOperationException("PostgreSQL health check query failed");
    }

    private async Task ValidateRedisAsync(CancellationToken cancellationToken)
    {
        // Build connection string from environment variables
        var host = Environment.GetEnvironmentVariable("REDIS_HOST");
        var portStr = Environment.GetEnvironmentVariable("REDIS_PORT");
        var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        if (string.IsNullOrEmpty(host))
            throw new InvalidOperationException("REDIS_HOST not configured");

        if (!int.TryParse(portStr ?? "6379", out var port))
            throw new InvalidOperationException("REDIS_PORT must be a valid integer");

        var configBuilder = new StringBuilder();
        configBuilder.Append($"{host}:{port}");

        if (!string.IsNullOrEmpty(password)) configBuilder.Append($",password={password}");

        var redis = await ConnectionMultiplexer.ConnectAsync(configBuilder.ToString());

        if (!redis.IsConnected) throw new InvalidOperationException("Redis connection failed: not connected");

        // Verify we can execute commands
        var db = redis.GetDatabase();
        var pingResult = await db.PingAsync();

        if (pingResult == TimeSpan.Zero) throw new InvalidOperationException("Redis health check ping failed");

        await redis.CloseAsync();
    }

    private async Task ValidateMinioAsync(CancellationToken cancellationToken)
    {
        var host = Environment.GetEnvironmentVariable("MINIO_HOST");
        var portStr = Environment.GetEnvironmentVariable("MINIO_PORT");
        var accessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        var secretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");
        var useSslStr = Environment.GetEnvironmentVariable("MINIO_USE_SSL");

        if (string.IsNullOrEmpty(host))
            throw new InvalidOperationException("MINIO_HOST not configured");
        if (string.IsNullOrEmpty(portStr))
            throw new InvalidOperationException("MINIO_PORT not configured");
        if (string.IsNullOrEmpty(accessKey))
            throw new InvalidOperationException("MINIO_ACCESS_KEY not configured");
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("MINIO_SECRET_KEY not configured");

        var useSSL = useSslStr != null && bool.Parse(useSslStr);
        var endpoint = $"{host}:{portStr}";

        var minio = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey);

        if (useSSL) minio = minio.WithSSL();

        var client = minio.Build();

        // Try to list buckets to verify connection and credentials
        var buckets = await client.ListBucketsAsync(cancellationToken);

        if (buckets == null) throw new InvalidOperationException("MinIO health check failed: cannot list buckets");
    }
}