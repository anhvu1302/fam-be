using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FAM.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Design-time factory for PostgreSqlDbContext so EF tools can create migrations.
/// </summary>
public class DesignTimePostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlDbContext>
{
    public PostgreSqlDbContext CreateDbContext(string[] args)
    {
        // Prefer environment variable for design-time. If not set, use a sensible default for local development.
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
                               ?? "Host=localhost;Database=fam;Username=postgres;Password=postgres";

        var options = new Common.Options.PostgreSqlOptions
        {
            ConnectionString = connectionString,
            EnableDetailedErrors = false,
            EnableSensitiveDataLogging = false
        };

        return new PostgreSqlDbContext(options);
    }
}
