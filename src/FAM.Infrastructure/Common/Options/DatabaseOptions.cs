namespace FAM.Infrastructure.Common.Options;

/// <summary>
/// Database configuration options
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.PostgreSQL;

    // PostgreSQL options
    public PostgreSqlOptions PostgreSql { get; set; } = new();

    // MongoDB options
    public MongoDbOptions MongoDb { get; set; } = new();
}

/// <summary>
/// Supported database providers
/// </summary>
public enum DatabaseProvider
{
    PostgreSQL,
    MongoDB
}

/// <summary>
/// PostgreSQL specific options
/// </summary>
public class PostgreSqlOptions
{
    public string ConnectionString { get; set; } = "Host=localhost;Database=fam;Username=postgres;Password=password";
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}

/// <summary>
/// MongoDB specific options
/// </summary>
public class MongoDbOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "fam";
}