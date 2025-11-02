namespace FAM.WebApi.Configuration;

/// <summary>
/// Centralized configuration loaded from environment variables.
/// Validates all required settings on application startup.
/// </summary>
public class AppConfiguration
{
    // Database
    public string DatabaseProvider { get; }
    public string PostgresConnectionString { get; }
    public string MongoDbConnectionString { get; }
    public string MongoDbDatabase { get; }

    // Database Components (for validation)
    public string DbHost { get; }
    public int DbPort { get; }
    public string DbName { get; }
    public string DbUser { get; }
    public string DbPassword { get; }

    // Authentication
    public string JwtSecret { get; }
    public string JwtIssuer { get; }
    public string JwtAudience { get; }
    public int AccessTokenExpirationMinutes { get; }
    public int RefreshTokenExpirationDays { get; }
    public int MaxFailedLoginAttempts { get; }
    public int AccountLockoutMinutes { get; }

    // Application
    public string Environment { get; }
    public int MaxPageSize { get; }
    public bool EnableDetailedErrors { get; }
    public bool EnableSensitiveDataLogging { get; }

    // Trusted Proxies
    public List<string> TrustedProxies { get; }
    public List<string> HeaderPriority { get; }
    public bool LogIpDetection { get; }

    public AppConfiguration()
    {
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
        MongoDbDatabase = DbName; // Use DB_NAME for MongoDB database name too

        // Authentication (Required)
        JwtSecret = GetRequired("JWT_SECRET");
        ValidateJwtSecret(JwtSecret);
        
        JwtIssuer = GetOptional("JWT_ISSUER") ?? "FAM.API";
        JwtAudience = GetOptional("JWT_AUDIENCE") ?? "FAM.Client";
        AccessTokenExpirationMinutes = GetInt("ACCESS_TOKEN_EXPIRATION_MINUTES", 60);
        RefreshTokenExpirationDays = GetInt("REFRESH_TOKEN_EXPIRATION_DAYS", 30);
        MaxFailedLoginAttempts = GetInt("MAX_FAILED_LOGIN_ATTEMPTS", 5);
        AccountLockoutMinutes = GetInt("ACCOUNT_LOCKOUT_MINUTES", 15);

        // Application
        Environment = GetOptional("ASPNETCORE_ENVIRONMENT") ?? "Development";
        MaxPageSize = GetInt("MAX_PAGE_SIZE", 100);
        EnableDetailedErrors = GetBool("ENABLE_DETAILED_ERRORS", Environment == "Development");
        EnableSensitiveDataLogging = GetBool("ENABLE_SENSITIVE_DATA_LOGGING", false);

        // Trusted Proxies
        TrustedProxies = GetList("TRUSTED_PROXIES", new[] { "127.0.0.1", "::1" });
        HeaderPriority = GetList("HEADER_PRIORITY", new[] 
        { 
            "CF-Connecting-IP", 
            "True-Client-IP", 
            "X-Real-IP", 
            "X-Forwarded-For", 
            "X-Client-IP" 
        });
        LogIpDetection = GetBool("LOG_IP_DETECTION", false);
    }

    private string BuildPostgresConnectionString()
    {
        // Build PostgreSQL connection string with proper format
        var connectionString = $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword}";
        
        // Validate connection string format
        if (!connectionString.Contains("Host=") || 
            !connectionString.Contains("Database=") || 
            !connectionString.Contains("Username="))
        {
            throw new InvalidOperationException(
                "Invalid PostgreSQL connection string format. " +
                "Required components: Host, Database, Username");
        }

        return connectionString;
    }

    private string BuildMongoDbConnectionString()
    {
        // Build MongoDB connection string
        // Format: mongodb://[username:password@]host[:port]/[database]
        
        // MongoDB requires escaping special characters in username/password
        var escapedUser = !string.IsNullOrEmpty(DbUser) ? Uri.EscapeDataString(DbUser) : "";
        var escapedPassword = !string.IsNullOrEmpty(DbPassword) ? Uri.EscapeDataString(DbPassword) : "";
        
        if (!string.IsNullOrEmpty(escapedUser) && !string.IsNullOrEmpty(escapedPassword))
        {
            return $"mongodb://{escapedUser}:{escapedPassword}@{DbHost}:{DbPort}";
        }
        else
        {
            return $"mongodb://{DbHost}:{DbPort}";
        }
    }

    private static string GetRequired(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable '{key}' is not set. " +
                $"Please check your .env file or environment configuration.");
        }
        return value;
    }

    private static string? GetOptional(string key)
    {
        return System.Environment.GetEnvironmentVariable(key);
    }

    private static int GetInt(string key, int defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        if (!int.TryParse(value, out var result))
        {
            throw new InvalidOperationException(
                $"Environment variable '{key}' has invalid integer value: '{value}'");
        }

        return result;
    }

    private static bool GetBool(string key, bool defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        if (!bool.TryParse(value, out var result))
        {
            throw new InvalidOperationException(
                $"Environment variable '{key}' has invalid boolean value: '{value}'. Use 'true' or 'false'.");
        }

        return result;
    }

    private static List<string> GetList(string key, string[] defaultValues)
    {
        var value = System.Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>(defaultValues);

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => x.Trim())
                   .ToList();
    }

    private static void ValidateJwtSecret(string secret)
    {
        if (secret.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT_SECRET must be at least 32 characters long for security reasons.");
        }
    }

    private static void ValidatePort(int port)
    {
        if (port < 1 || port > 65535)
        {
            throw new InvalidOperationException(
                $"DB_PORT must be between 1 and 65535. Current value: {port}");
        }
    }

    private static void ValidateDatabaseProvider(string provider)
    {
        var validProviders = new[] { "PostgreSQL", "MongoDB" };
        if (!validProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"DB_PROVIDER must be either 'PostgreSQL' or 'MongoDB'. Current value: '{provider}'");
        }
    }

    public void LogConfiguration(ILogger logger)
    {
        logger.LogInformation("=== Application Configuration ===");
        logger.LogInformation("Environment: {Environment}", Environment);
        logger.LogInformation("Database Provider: {Provider}", DatabaseProvider);
        logger.LogInformation("Database: {Host}:{Port}/{Database} (User: {User})", 
            DbHost, DbPort, DbName, DbUser);
        
        logger.LogInformation("JWT: Issuer={Issuer}, Audience={Audience}", JwtIssuer, JwtAudience);
        logger.LogInformation("JWT: AccessToken={AccessMinutes}min, RefreshToken={RefreshDays}days", 
            AccessTokenExpirationMinutes, RefreshTokenExpirationDays);
        logger.LogInformation("Paging: MaxPageSize={MaxPageSize}", MaxPageSize);
        logger.LogInformation("DetailedErrors: {Enabled}, SensitiveDataLogging: {Enabled2}", 
            EnableDetailedErrors, EnableSensitiveDataLogging);
        logger.LogInformation("==================================");
    }
}
