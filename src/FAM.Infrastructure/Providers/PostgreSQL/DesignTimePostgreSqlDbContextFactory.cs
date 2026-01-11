using FAM.Infrastructure.Common.Options;

using Microsoft.EntityFrameworkCore.Design;

namespace FAM.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Design-time factory for PostgreSqlDbContext so EF tools can create migrations.
/// Loads configuration from .env file and environment variables.
/// </summary>
public class DesignTimePostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlDbContext>
{
    public PostgreSqlDbContext CreateDbContext(string[] args)
    {
        // Load .env file from project root
        LoadDotEnv();

        // Get connection components from environment variables
        string dbHost = Environment.GetEnvironmentVariable("DB_HOST")
                        ?? throw new InvalidOperationException("DB_HOST environment variable is not set.");
        string dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        string dbName = Environment.GetEnvironmentVariable("DB_NAME")
                        ?? throw new InvalidOperationException("DB_NAME environment variable is not set.");
        string dbUser = Environment.GetEnvironmentVariable("DB_USER")
                        ?? throw new InvalidOperationException("DB_USER environment variable is not set.");
        string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                            ?? throw new InvalidOperationException("DB_PASSWORD environment variable is not set.");

        // Build PostgreSQL connection string
        string connectionString =
            $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

        // Validate port
        if (!int.TryParse(dbPort, out int port) || port < 1 || port > 65535)
        {
            throw new InvalidOperationException(
                $"DB_PORT must be a valid port number (1-65535). Current value: {dbPort}");
        }

        bool enableDetailedErrors = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_DETAILED_ERRORS") ?? "false");
        bool enableSensitiveDataLogging =
            bool.Parse(Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING") ?? "false");

        PostgreSqlOptions options = new()
        {
            ConnectionString = connectionString,
            EnableDetailedErrors = enableDetailedErrors,
            EnableSensitiveDataLogging = enableSensitiveDataLogging
        };

        Console.WriteLine($"Using PostgreSQL connection: {dbHost}:{dbPort}/{dbName} as {dbUser}");

        return new PostgreSqlDbContext(options);
    }

    private static void LoadDotEnv()
    {
        // Find the solution root directory (where .env is located)
        string currentDir = Directory.GetCurrentDirectory();
        string? solutionRoot = FindSolutionRoot(currentDir);

        if (solutionRoot == null)
        {
            Console.WriteLine("Warning: Could not find solution root. Skipping .env file loading.");
            return;
        }

        string envFile = Path.Combine(solutionRoot, ".env");

        if (!File.Exists(envFile))
        {
            Console.WriteLine($"Warning: .env file not found at '{envFile}'. Using system environment variables only.");
            return;
        }

        Console.WriteLine($"Loading .env file from: {envFile}");

        foreach (string line in File.ReadAllLines(envFile))
        {
            string trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            string[] parts = trimmedLine.Split('=', 2, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            // Remove quotes if present
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value[1..^1];
            }

            // Only set if not already set (system env vars take precedence)
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        Console.WriteLine(".env file loaded successfully");
    }

    private static string? FindSolutionRoot(string startDirectory)
    {
        DirectoryInfo? current = new(startDirectory);

        while (current != null)
        {
            // Look for .sln file or .env file as indicators of solution root
            if (current.GetFiles("*.sln").Length > 0 ||
                current.GetFiles(".env").Length > 0)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}
