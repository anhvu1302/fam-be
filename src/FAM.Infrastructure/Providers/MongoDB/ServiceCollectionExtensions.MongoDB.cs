using FAM.Domain.Abstractions;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.MongoDB.Repositories;
using FAM.Infrastructure.Providers.MongoDB.Seeders;

using Microsoft.Extensions.DependencyInjection;

namespace FAM.Infrastructure.Providers.MongoDB;

/// <summary>
/// Extension methods for configuring MongoDB provider
/// </summary>
public static class ServiceCollectionExtensions
{
    public static async Task<IServiceCollection> AddMongoDbAsync(this IServiceCollection services,
        MongoDbOptions options)
    {
        // Register MongoDB context
        services.AddSingleton<MongoDbContext>(_ => new MongoDbContext(options));

        // Register class maps
        MongoClassMaps.Register();

        // Create indexes
        var context = new MongoDbContext(options);
        await MongoIndexes.CreateIndexesAsync(context.Database);

        // Register repositories
        services.AddScoped<IUserRepository, UserRepositoryMongo>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepositoryMongo>();
        services.AddScoped<ISigningKeyRepository, SigningKeyRepositoryMongo>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWorkMongo>();

        // Register Data Seeders
        services.AddScoped<IDataSeeder, MongoDbSigningKeySeeder>();
        services.AddScoped<IDataSeeder, MongoDbUserSeeder>();
        services.AddScoped<IDataSeeder, MongoDbRoleSeeder>();

        // Register Seed History Repository
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepositoryMongo>();

        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, MongoDbOptions options)
    {
        // Synchronous version - indexes will be created on first access
        services.AddSingleton<MongoDbContext>(_ => new MongoDbContext(options));
        MongoClassMaps.Register();

        services.AddScoped<IUserRepository, UserRepositoryMongo>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepositoryMongo>();
        services.AddScoped<ISigningKeyRepository, SigningKeyRepositoryMongo>();
        services.AddScoped<IUnitOfWork, UnitOfWorkMongo>();

        // Register Data Seeders
        services.AddScoped<IDataSeeder, MongoDbSigningKeySeeder>();
        services.AddScoped<IDataSeeder, MongoDbUserSeeder>();
        services.AddScoped<IDataSeeder, MongoDbRoleSeeder>();

        // Register Seed History Repository
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepositoryMongo>();

        return services;
    }
}
