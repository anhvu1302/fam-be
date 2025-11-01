using FAM.Application.Common.Mappings;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Infrastructure.Common.Mapping;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.MongoDB;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FAM.Infrastructure;

/// <summary>
/// Infrastructure module - configures the appropriate database provider
/// </summary>
public static class InfrastructureModule
{
    public static async Task<IServiceCollection> AddInfrastructureAsync(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure database options
        var databaseOptionsSection = configuration.GetSection(DatabaseOptions.SectionName);
        var databaseOptions = new DatabaseOptions
        {
            Provider = Enum.Parse<DatabaseProvider>(databaseOptionsSection["Provider"] ?? "PostgreSQL"),
            PostgreSql = new PostgreSqlOptions
            {
                ConnectionString = databaseOptionsSection.GetSection("PostgreSql")["ConnectionString"] ?? ""
            },
            MongoDb = new MongoDbOptions
            {
                ConnectionString = databaseOptionsSection.GetSection("MongoDb")["ConnectionString"] ?? "",
                DatabaseName = databaseOptionsSection.GetSection("MongoDb")["DatabaseName"] ?? ""
            }
        };

        // Register AutoMapper with specific profiles
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DomainToEfProfile>();
            cfg.AddProfile<DomainToMongoProfile>();
            cfg.AddProfile<EfToDtoProfile>();
            cfg.AddProfile<MongoToDtoProfile>();
            cfg.AddProfile<UserMappingProfile>();
        });

        // Register Data Seeder Orchestrator
        services.AddScoped<DataSeederOrchestrator>();

        // Register Location Service with HttpClient
        services.AddHttpClient<ILocationService, IpApiLocationService>();

        // Configure based on provider
        switch (databaseOptions.Provider)
        {
            case DatabaseProvider.PostgreSQL:
                services.AddPostgreSql(databaseOptions.PostgreSql);
                break;

            case DatabaseProvider.MongoDB:
                await services.AddMongoDbAsync(databaseOptions.MongoDb);
                break;

            default:
                throw new InvalidOperationException($"Unsupported database provider: {databaseOptions.Provider}");
        }

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Synchronous version
        var databaseOptionsSection = configuration.GetSection(DatabaseOptions.SectionName);
        var databaseOptions = new DatabaseOptions
        {
            Provider = Enum.Parse<DatabaseProvider>(databaseOptionsSection["Provider"] ?? "PostgreSQL"),
            PostgreSql = new PostgreSqlOptions
            {
                ConnectionString = databaseOptionsSection.GetSection("PostgreSql")["ConnectionString"] ?? ""
            },
            MongoDb = new MongoDbOptions
            {
                ConnectionString = databaseOptionsSection.GetSection("MongoDb")["ConnectionString"] ?? "",
                DatabaseName = databaseOptionsSection.GetSection("MongoDb")["DatabaseName"] ?? ""
            }
        };

        // Register AutoMapper with specific profiles
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DomainToEfProfile>();
            cfg.AddProfile<DomainToMongoProfile>();
            cfg.AddProfile<EfToDtoProfile>();
            cfg.AddProfile<MongoToDtoProfile>();
            cfg.AddProfile<UserMappingProfile>();
        });

        // Register Data Seeder Orchestrator
        services.AddScoped<DataSeederOrchestrator>();

        // Register Location Service with HttpClient
        services.AddHttpClient<ILocationService, IpApiLocationService>();

        switch (databaseOptions.Provider)
        {
            case DatabaseProvider.PostgreSQL:
                services.AddPostgreSql(databaseOptions.PostgreSql);
                break;

            case DatabaseProvider.MongoDB:
                services.AddMongoDb(databaseOptions.MongoDb);
                break;

            default:
                throw new InvalidOperationException($"Unsupported database provider: {databaseOptions.Provider}");
        }

        return services;
    }
}