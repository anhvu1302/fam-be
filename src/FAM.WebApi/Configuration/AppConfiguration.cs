namespace FAM.WebApi.Configuration;

/// <summary>
/// Centralized configuration for SENSITIVE settings loaded from environment variables.
/// Non-sensitive settings are loaded from appsettings.json via IConfiguration.
/// </summary>
public class AppConfiguration
{
    // Database (from .env - sensitive)
    public string DatabaseProvider { get; }
    public string PostgresConnectionString { get; }
    public string MongoDbConnectionString { get; }
    public string MongoDbDatabase { get; }

    // Database Components
    public string DbHost { get; }
    public int DbPort { get; }
    public string DbName { get; }
    public string DbUser { get; }
    public string DbPassword { get; }

    // Authentication (from .env - sensitive)
    public string JwtSecret { get; }
    public string JwtIssuer { get; }
    public string JwtAudience { get; }
    public int AccessTokenExpirationMinutes { get; }
    public int RefreshTokenExpirationDays { get; }
    public int MaxFailedLoginAttempts { get; }
    public int AccountLockoutMinutes { get; }

    // MinIO (from .env - sensitive)
    public string MinioEndpoint { get; }
    public string MinioAccessKey { get; }
    public string MinioSecretKey { get; }
    public bool MinioUseSsl { get; }
    public string MinioBucketName { get; }

    // Environment
    public string Environment { get; }
    public bool IsDevelopment => Environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

    public AppConfiguration()
    {
        // Environment
        Environment = GetOptional("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Database Provider
        DatabaseProvider = GetOptional("DB_PROVIDER") ?? "PostgreSQL";
        ValidateDatabaseProvider(DatabaseProvider);

        // Database Connection Components (Required)
        DbHost = GetRequired("DB_HOST");
        DbPort = GetInt("DB_PORT", 5432);
        ValidatePort(DbPort);

        DbName = GetRequired("DB_NAME");
        DbUser = GetRequired("DB_USER");
        DbPassword = GetRequired("DB_PASSWORD");

        // Build connection strings based on provider
        PostgresConnectionString = BuildPostgresConnectionString();
        MongoDbConnectionString = BuildMongoDbConnectionString();
        MongoDbDatabase = DbName;

        // Authentication (Required - Sensitive)
        JwtSecret = GetRequired("JWT_SECRET");
        ValidateJwtSecret(JwtSecret);

        JwtIssuer = GetOptional("JWT_ISSUER") ?? "FAM.API";
        JwtAudience = GetOptional("JWT_AUDIENCE") ?? "FAM.Client";
        AccessTokenExpirationMinutes = GetInt("ACCESS_TOKEN_EXPIRATION_MINUTES", 60);
        RefreshTokenExpirationDays = GetInt("REFRESH_TOKEN_EXPIRATION_DAYS", 30);
        MaxFailedLoginAttempts = GetInt("MAX_FAILED_LOGIN_ATTEMPTS", 5);
        AccountLockoutMinutes = GetInt("ACCOUNT_LOCKOUT_MINUTES", 15);

        // MinIO (Required - Sensitive)
        MinioEndpoint = GetOptional("MINIO_ENDPOINT") ?? "localhost:9000";
        MinioAccessKey = GetRequired("MINIO_ACCESS_KEY");
        MinioSecretKey = GetRequired("MINIO_SECRET_KEY");
        MinioUseSsl = GetBool("MINIO_USE_SSL", false);
        MinioBucketName = GetOptional("MINIO_BUCKET_NAME") ?? "fam-assets";
    }

    private string BuildPostgresConnectionString()
    {
        return $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword}";
    }

    private string BuildMongoDbConnectionString()
    {
        var escapedUser = !string.IsNullOrEmpty(DbUser) ? Uri.EscapeDataString(DbUser) : "";
        var escapedPassword = !string.IsNullOrEmpty(DbPassword) ? Uri.EscapeDataString(DbPassword) : "";

        if (!string.IsNullOrEmpty(escapedUser) && !string.IsNullOrEmpty(escapedPassword))
            return $"mongodb://{escapedUser}:{escapedPassword}@{DbHost}:{DbPort}";
        return $"mongodb://{DbHost}:{DbPort}";
    }

    private static string GetRequired(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Required environment variable '{key}' is not set. Check your .env file.");
        return value;
    }

    private static string? GetOptional(string key)
    {
        return System.Environment.GetEnvironmentVariable(key);
    }

    private static int GetInt(string key, int defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (!int.TryParse(value, out var result))
            throw new InvalidOperationException($"'{key}' must be a valid integer. Got: '{value}'");
        return result;
    }

    private static bool GetBool(string key, bool defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (!bool.TryParse(value, out var result))
            throw new InvalidOperationException($"'{key}' must be 'true' or 'false'. Got: '{value}'");
        return result;
    }

    private static void ValidateJwtSecret(string secret)
    {
        if (secret.Length < 32)
            throw new InvalidOperationException("JWT_SECRET must be at least 32 characters.");
    }

    private static void ValidatePort(int port)
    {
        if (port < 1 || port > 65535)
            throw new InvalidOperationException($"DB_PORT must be 1-65535. Got: {port}");
    }

    private static void ValidateDatabaseProvider(string provider)
    {
        var valid = new[] { "PostgreSQL", "MongoDB" };
        if (!valid.Contains(provider, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"DB_PROVIDER must be 'PostgreSQL' or 'MongoDB'. Got: '{provider}'");
    }

    public void LogConfiguration(ILogger logger)
    {
        logger.LogInformation("=== FAM Configuration ===");
        logger.LogInformation("Environment: {Env}", Environment);
        logger.LogInformation("Database: {Provider} @ {Host}:{Port}/{Db}",
            DatabaseProvider, DbHost, DbPort, DbName);
        logger.LogInformation("MinIO: {Endpoint}, Bucket: {Bucket}, SSL: {Ssl}",
            MinioEndpoint, MinioBucketName, MinioUseSsl);
        logger.LogInformation("JWT: Issuer={Issuer}, AccessToken={Min}min, RefreshToken={Days}days",
            JwtIssuer, AccessTokenExpirationMinutes, RefreshTokenExpirationDays);
        logger.LogInformation("=========================");
    }
}