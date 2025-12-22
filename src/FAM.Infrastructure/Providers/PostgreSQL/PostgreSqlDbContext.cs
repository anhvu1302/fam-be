using System.Reflection;

using FAM.Domain.Assets;
using FAM.Domain.Authorization;
using FAM.Domain.Categories;
using FAM.Domain.Common.Entities;
using FAM.Domain.Conditions;
using FAM.Domain.EmailTemplates;
using FAM.Domain.Finance;
using FAM.Domain.Geography;
using FAM.Domain.Locations;
using FAM.Domain.Manufacturers;
using FAM.Domain.Models;
using FAM.Domain.Organizations;
using FAM.Domain.Statuses;
using FAM.Domain.Storage;
using FAM.Domain.Suppliers;
using FAM.Domain.Types;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;
using FAM.Infrastructure.Common.Extensions;
using FAM.Infrastructure.Common.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FAM.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// PostgreSQL DbContext for EF Core
/// </summary>
public class PostgreSqlDbContext : DbContext
{
    private readonly PostgreSqlOptions _options;

    public PostgreSqlDbContext(PostgreSqlOptions options)
    {
        _options = options;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    public DbSet<UserTheme> UserThemes { get; set; }

    // Authorization entities - Using Domain entities directly (Pragmatic Architecture)
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserNodeRole> UserNodeRoles { get; set; }

    // Organizations entities
    public DbSet<OrgNode> OrgNodes { get; set; }
    public DbSet<CompanyDetails> CompanyDetails { get; set; }
    public DbSet<DepartmentDetails> DepartmentDetails { get; set; }

    // Asset entities
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetEvent> AssetEvents { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<FinanceEntry> FinanceEntries { get; set; }

    // Categories & Types
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<AssetType> AssetTypes { get; set; }

    // Status entities
    public DbSet<UsageStatus> UsageStatuses { get; set; }
    public DbSet<LifecycleStatus> LifecycleStatuses { get; set; }
    public DbSet<AssetEventType> AssetEventTypes { get; set; }
    public DbSet<AssetCondition> AssetConditions { get; set; }

    // Storage entities
    public DbSet<UploadSession> UploadSessions { get; set; }

    // Geography & Location
    public DbSet<Country> Countries { get; set; }
    public DbSet<Location> Locations { get; set; }

    // Manufacturers & Models
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Model> Models { get; set; }

    // Suppliers
    public DbSet<Supplier> Suppliers { get; set; }

    // Security
    public DbSet<SigningKey> SigningKeys { get; set; }

    // UI & Configuration
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    // Email
    public DbSet<EmailTemplate> EmailTemplates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_options.ConnectionString, npgsqlOptions =>
            {
                // store migrations history table without schema
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
            })
            .UseSnakeCaseNamingConvention();

        if (_options.EnableDetailedErrors)
            optionsBuilder.EnableDetailedErrors();

        if (_options.EnableSensitiveDataLogging)
            optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore audit navigation properties globally (they are for tracking only, not FK constraints)
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Ignore CreatedBy, UpdatedBy, DeletedBy navigation properties
            IEnumerable<PropertyInfo> auditNavProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(User) &&
                            (p.Name == "CreatedBy" || p.Name == "UpdatedBy" || p.Name == "DeletedBy"));

            foreach (PropertyInfo navProperty in auditNavProperties)
                modelBuilder.Entity(entityType.ClrType).Ignore(navProperty.Name);
        }

        // Auto-load all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply snake_case naming convention
        ModelBuilderExtensions.ApplySnakeCaseNamingConvention(modelBuilder);
    }
}
