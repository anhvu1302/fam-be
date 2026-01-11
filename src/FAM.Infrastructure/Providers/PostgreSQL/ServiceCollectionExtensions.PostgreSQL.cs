using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Infrastructure.Common.Abstractions;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Repositories;
using FAM.Infrastructure.Seeders;

using Microsoft.Extensions.DependencyInjection;

namespace FAM.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Extension methods for configuring PostgreSQL provider
/// Implements Clean Architecture by registering abstractions
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSql(this IServiceCollection services, string connectionString)
    {
        PostgreSqlOptions options = new() { ConnectionString = connectionString };
        return AddPostgreSql(services, options);
    }

    public static IServiceCollection AddPostgreSql(this IServiceCollection services, PostgreSqlOptions options)
    {
        // Register DbContext - implements IDbContext for abstraction
        services.AddScoped<PostgreSqlDbContext>(_ => new PostgreSqlDbContext(options));

        // Register IDbContext abstraction pointing to concrete implementation
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<PostgreSqlDbContext>());

        // Register DbContext Factory for clean architecture
        services.AddScoped<IDbContextFactory>(_ => new PostgreSqlDbContextFactory(options));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<IUserThemeRepository, UserThemeRepository>();
        services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

        // Authorization repositories
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserNodeRoleRepository, UserNodeRoleRepository>();

        // Organizations repositories
        services.AddScoped<IOrgNodeRepository, OrgNodeRepository>();
        services.AddScoped<ICompanyDetailsRepository, CompanyDetailsRepository>();
        services.AddScoped<IDepartmentDetailsRepository, DepartmentDetailsRepository>();

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
        services.AddScoped<IDataSeeder, EmailTemplateSeeder>();

        // Register Seed History Repository
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepository>();

        return services;
    }
}
