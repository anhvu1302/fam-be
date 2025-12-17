using FAM.Application.Abstractions;
using FAM.Application.Common.Services;
using FAM.Application.Storage;
using FAM.Domain.Abstractions;
using FAM.Infrastructure.Auth;
using FAM.Infrastructure.Common.Mapping;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
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
        // Get database provider - REQUIRED, no default
        var providerStr = Environment.GetEnvironmentVariable("DB_PROVIDER");
        if (string.IsNullOrEmpty(providerStr))
            throw new InvalidOperationException(
                "DB_PROVIDER environment variable is required. Valid value: 'PostgreSQL'");

        // Validate provider
        if (!Enum.TryParse<DatabaseProvider>(providerStr, true, out DatabaseProvider provider))
            throw new InvalidOperationException(
                $"Invalid DB_PROVIDER value: '{providerStr}'. Valid value: 'PostgreSQL'");

        // Get database connection parameters
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "fam_db";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

        // Build connection string based on provider
        string connectionString;
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                connectionString =
                    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
                break;

            default:
                throw new InvalidOperationException($"Unsupported database provider: {provider}");
        }

        // Register AutoMapper with specific profiles (Domain <-> Persistence layer mappings)
        services.AddAutoMapper(cfg => { cfg.AddProfile<DomainToEfProfile>(); });

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
        services.AddScoped<IFileValidator, FileValidator>();

        // Register Signing Key Service
        services.AddScoped<ISigningKeyService, SigningKeyService>();

        // Register PostgreSQL database provider
        services.AddPostgreSql(connectionString);

        return services;
    }
}
