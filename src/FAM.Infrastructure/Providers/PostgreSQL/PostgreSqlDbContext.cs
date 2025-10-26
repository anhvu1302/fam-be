using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<CompanyEf> Companies { get; set; }
    public DbSet<UserEf> Users { get; set; }
    public DbSet<AssetEf> Assets { get; set; }
    public DbSet<LocationEf> Locations { get; set; }
    public DbSet<SupplierEf> Suppliers { get; set; }
    public DbSet<FinanceEntryEf> FinanceEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_options.ConnectionString, npgsqlOptions =>
        {
            // store migrations history table without schema
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
        });

        if (_options.EnableDetailedErrors)
            optionsBuilder.EnableDetailedErrors();

        if (_options.EnableSensitiveDataLogging)
            optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply centralized snake_case naming convention (tables, columns, constraints, indexes)
        FAM.Infrastructure.Common.Extensions.ModelBuilderExtensions.ApplySnakeCaseNamingConvention(modelBuilder);

        // Company configuration
        modelBuilder.Entity<CompanyEf>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.TaxCode).HasMaxLength(50);
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.IsDeleted).HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(c => !c.IsDeleted);

            // Indexes - explicitly name them to ensure snake_case
            entity.HasIndex(c => c.Name).HasDatabaseName("ix_companies_name");
            entity.HasIndex(c => c.TaxCode).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_companies_tax_code");

            // Audit user relationships
            entity.HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.UpdatedBy)
                .WithMany()
                .HasForeignKey(c => c.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.DeletedBy)
                .WithMany()
                .HasForeignKey(c => c.DeletedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // User configuration
        modelBuilder.Entity<UserEf>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.IsDeleted).HasDefaultValue(false);

            // Soft delete filter
            entity.HasQueryFilter(u => !u.IsDeleted);

            // Indexes - explicitly name them to ensure snake_case
            entity.HasIndex(u => u.Username).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_users_username");
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_users_email");
        });

        // Asset configuration
        modelBuilder.Entity<AssetEf>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired();

            // Relationships
            entity.HasOne(a => a.Company)
                .WithMany(c => c.Assets)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit user relationships
            entity.HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.UpdatedBy)
                .WithMany()
                .HasForeignKey(a => a.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.DeletedBy)
                .WithMany()
                .HasForeignKey(a => a.DeletedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Location configuration
        modelBuilder.Entity<LocationEf>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired();

            // Relationships
            entity.HasOne(l => l.Company)
                .WithMany(c => c.Locations)
                .HasForeignKey(l => l.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit user relationships
            entity.HasOne(l => l.CreatedBy)
                .WithMany()
                .HasForeignKey(l => l.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(l => l.UpdatedBy)
                .WithMany()
                .HasForeignKey(l => l.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(l => l.DeletedBy)
                .WithMany()
                .HasForeignKey(l => l.DeletedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Supplier configuration
        modelBuilder.Entity<SupplierEf>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired();

            // Relationships
            entity.HasOne(s => s.Company)
                .WithMany(c => c.Suppliers)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit user relationships
            entity.HasOne(s => s.CreatedBy)
                .WithMany()
                .HasForeignKey(s => s.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.UpdatedBy)
                .WithMany()
                .HasForeignKey(s => s.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.DeletedBy)
                .WithMany()
                .HasForeignKey(s => s.DeletedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // FinanceEntry configuration
        modelBuilder.Entity<FinanceEntryEf>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Description).IsRequired();
            entity.Property(f => f.Amount).IsRequired();

            // Relationships
            entity.HasOne(f => f.User)
                .WithMany(u => u.FinanceEntries)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit user relationships
            entity.HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(f => f.UpdatedBy)
                .WithMany()
                .HasForeignKey(f => f.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(f => f.DeletedBy)
                .WithMany()
                .HasForeignKey(f => f.DeletedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}