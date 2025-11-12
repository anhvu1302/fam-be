using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL.Repositories;
using FAM.Infrastructure.Providers.PostgreSQL.Seeders;
using FAM.Infrastructure.Providers.PostgreSQL.Services;
using FAM.Infrastructure.Repositories;
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
        services.AddScoped<IUserRepository, UserRepositoryPostgreSql>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepositoryPostgreSql>();

        // Authorization repositories
        services.AddScoped<IPermissionRepository, PermissionRepositoryPostgreSql>();
        services.AddScoped<IRoleRepository, RoleRepositoryPostgreSql>();
        services.AddScoped<IResourceRepository, ResourceRepositoryPostgreSql>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepositoryPostgreSql>();
        services.AddScoped<IUserNodeRoleRepository, UserNodeRoleRepositoryPostgreSql>();

        // Organizations repositories
        services.AddScoped<IOrgNodeRepository, OrgNodeRepositoryPostgreSql>();
        services.AddScoped<ICompanyDetailsRepository, CompanyDetailsRepositoryPostgreSql>();
        services.AddScoped<IDepartmentDetailsRepository, DepartmentDetailsRepositoryPostgreSql>();

        // Storage repositories
        services.AddScoped<IUploadSessionRepository, UploadSessionRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWorkPostgreSql>();

        // Register Query Services (for Filter DSL)
        services.AddScoped<IQueryService<UserDto>, UserQueryService>();

        // Register Data Seeders
        services.AddScoped<IDataSeeder, PostgreSqlAdminUserSeeder>();
        services.AddScoped<IDataSeeder, PostgreSqlRoleSeeder>();

        // Register Seed History Repository
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepositoryPostgreSql>();

        return services;
    }
}