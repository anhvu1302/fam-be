namespace FAM.Infrastructure.Common.Options;

/// <summary>
/// Supported database providers
/// </summary>
public enum DatabaseProvider
{
    PostgreSQL
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
