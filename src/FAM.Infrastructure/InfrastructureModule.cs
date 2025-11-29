using FAM.Application.Abstractions;
using FAM.Application.Auth.Services;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Infrastructure.Auth;
using FAM.Infrastructure.Common.Mapping;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.MongoDB;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace FAM.Infrastructure;

/// <summary>
/// Infrastructure module - configures the appropriate database provider
/// </summary>
public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Get database provider from environment or default to PostgreSQL
        var providerStr = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "PostgreSQL";

        // Validate provider
        if (!Enum.TryParse<DatabaseProvider>(providerStr, true, out var provider))
            throw new InvalidOperationException(
                $"Invalid DB_PROVIDER value: '{providerStr}'. Valid values are: 'PostgreSQL', 'MongoDB'");

        // Get shared database connection parameters
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ??
                     (provider == DatabaseProvider.MongoDB ? "27017" : "5432");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "fam_db";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

        // Build connection strings based on provider
        var postgresConnection = "";
        var mongoConnection = "";

        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                // Only build PostgreSQL connection string
                postgresConnection =
                    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
                break;

            case DatabaseProvider.MongoDB:
                // Only build MongoDB connection string
                // Format: mongodb://[username:password@]host[:port]/[database]
                var escapedUser = !string.IsNullOrEmpty(dbUser) ? Uri.EscapeDataString(dbUser) : "";
                var escapedPassword = !string.IsNullOrEmpty(dbPassword) ? Uri.EscapeDataString(dbPassword) : "";

                if (!string.IsNullOrEmpty(escapedUser) && !string.IsNullOrEmpty(escapedPassword))
                    mongoConnection = $"mongodb://{escapedUser}:{escapedPassword}@{dbHost}:{dbPort}";
                else
                    mongoConnection = $"mongodb://{dbHost}:{dbPort}";
                break;
        }

        var databaseOptions = new DatabaseOptions
        {
            Provider = provider,
            PostgreSql = new PostgreSqlOptions
            {
                ConnectionString = postgresConnection
            },
            MongoDb = new MongoDbOptions
            {
                ConnectionString = mongoConnection,
                DatabaseName = dbName
            }
        };

        // Register AutoMapper with specific profiles (Domain <-> Persistence layer mappings)
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DomainToEfProfile>();
            cfg.AddProfile<DomainToMongoProfile>();
        });

        // Register Data Seeder Orchestrator
        services.AddScoped<DataSeederOrchestrator>();

        // Register Location Service with HttpClient
        services.AddHttpClient<ILocationService, IpApiLocationService>();

        // Register MinIO Client
        services.AddSingleton<IMinioClient>(sp =>
        {
            var minioEndpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? "localhost:9000";
            var minioAccessKey = Environment.GetEnvironmentVariable("MINIO_ROOT_USER") ?? "minioadmin";
            var minioSecretKey = Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD") ?? "minioadmin";
            var minioUseSsl = bool.Parse(Environment.GetEnvironmentVariable("MINIO_USE_SSL") ?? "false");

            return new MinioClient()
                .WithEndpoint(minioEndpoint)
                .WithCredentials(minioAccessKey, minioSecretKey)
                .WithSSL(minioUseSsl)
                .Build();
        });

        // Register Storage Services
        services.AddScoped<IStorageService, MinioStorageService>();
        services.AddScoped<IFileValidator, Application.Storage.FileValidator>();

        // Register Signing Key Service
        services.AddScoped<ISigningKeyService, SigningKeyService>();

        // Register only the selected database provider
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                services.AddPostgreSql(databaseOptions.PostgreSql);
                break;

            case DatabaseProvider.MongoDB:
                services.AddMongoDb(databaseOptions.MongoDb);
                break;

            default:
                throw new InvalidOperationException($"Unsupported database provider: {provider}");
        }

        return services;
    }
}