using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL.Repositories;
using FAM.Infrastructure.Providers.PostgreSQL.Seeders;
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
        services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

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

        // Register Data Seeders
        services.AddScoped<IDataSeeder, SigningKeySeeder>();
        services.AddScoped<IDataSeeder, AdminUserSeeder>();
        services.AddScoped<IDataSeeder, RoleSeeder>();
        services.AddScoped<IDataSeeder, PermissionSeeder>(); // New
        services.AddScoped<IDataSeeder, RolePermissionSeeder>(); // New
        services.AddScoped<IDataSeeder, LifecycleStatusSeeder>();
        services.AddScoped<IDataSeeder, UsageStatusSeeder>();
        services.AddScoped<IDataSeeder, CountrySeeder>();
        services.AddScoped<IDataSeeder, OrganizationSeeder>();
        services.AddScoped<IDataSeeder, MenuSeeder>();
        services.AddScoped<IDataSeeder, SystemSettingSeeder>();

        // Register Seed History Repository
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepositoryPostgreSql>();

        return services;
    }
}