using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Domain.Abstractions;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Providers.PostgreSQL.Repositories;
using FAM.Infrastructure.Providers.PostgreSQL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FAM.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Extension methods for configuring PostgreSQL provider
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSql(this IServiceCollection services, PostgreSqlOptions options)
    {
        // Register DbContext
        services.AddScoped<PostgreSqlDbContext>(_ => new PostgreSqlDbContext(options));

        // Register repositories
        services.AddScoped<ICompanyRepository, CompanyRepositoryPostgreSql>();
        services.AddScoped<IUserRepository, UserRepositoryPostgreSql>();
        // services.AddScoped<IAssetRepository, AssetRepositoryPostgreSql>(); // TODO: Implement missing methods
        // services.AddScoped<ILocationRepository, LocationRepositoryPostgreSql>(); // TODO: Implement missing methods

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWorkPostgreSql>();

        // Register Query Services (for Filter DSL)
        services.AddScoped<IQueryService<UserDto>, UserQueryService>();

        return services;
    }
}