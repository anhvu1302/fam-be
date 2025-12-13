using System.Reflection;

using FAM.Domain.Storage;
using FAM.Infrastructure.Common.Extensions;
using FAM.Infrastructure.Common.Options;
using FAM.Infrastructure.PersistenceModels.Ef;

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

    public DbSet<UserEf> Users { get; set; }
    public DbSet<UserDeviceEf> UserDevices { get; set; }
    public DbSet<UserThemeEf> UserThemes { get; set; }

    // Authorization entities
    public DbSet<PermissionEf> Permissions { get; set; }
    public DbSet<RoleEf> Roles { get; set; }
    public DbSet<ResourceEf> Resources { get; set; }
    public DbSet<RolePermissionEf> RolePermissions { get; set; }
    public DbSet<UserNodeRoleEf> UserNodeRoles { get; set; }

    // Organizations entities
    public DbSet<OrgNodeEf> OrgNodes { get; set; }
    public DbSet<CompanyDetailsEf> CompanyDetails { get; set; }
    public DbSet<DepartmentDetailsEf> DepartmentDetails { get; set; }

    // Asset entities
    public DbSet<AssetEf> Assets { get; set; }
    public DbSet<AssetEventEf> AssetEvents { get; set; }
    public DbSet<AssignmentEf> Assignments { get; set; }
    public DbSet<AttachmentEf> Attachments { get; set; }
    public DbSet<FinanceEntryEf> FinanceEntries { get; set; }

    // Categories & Types
    public DbSet<AssetCategoryEf> AssetCategories { get; set; }
    public DbSet<AssetTypeEf> AssetTypes { get; set; }

    // Status entities
    public DbSet<UsageStatusEf> UsageStatuses { get; set; }
    public DbSet<LifecycleStatusEf> LifecycleStatuses { get; set; }
    public DbSet<AssetEventTypeEf> AssetEventTypes { get; set; }
    public DbSet<AssetConditionEf> AssetConditions { get; set; }

    // Storage entities
    public DbSet<UploadSession> UploadSessions { get; set; }

    // Geography & Location
    public DbSet<CountryEf> Countries { get; set; }
    public DbSet<LocationEf> Locations { get; set; }

    // Manufacturers & Models
    public DbSet<ManufacturerEf> Manufacturers { get; set; }
    public DbSet<ModelEf> Models { get; set; }

    // Suppliers
    public DbSet<SupplierEf> Suppliers { get; set; }

    // Security
    public DbSet<SigningKeyEf> SigningKeys { get; set; }

    // UI & Configuration
    public DbSet<MenuItemEf> MenuItems { get; set; }
    public DbSet<SystemSettingEf> SystemSettings { get; set; }

    // Email
    public DbSet<EmailTemplateEf> EmailTemplates { get; set; }

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

        // Ignore audit navigation properties globally (they are for tracking only, not FK constraints)
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Ignore CreatedBy, UpdatedBy, DeletedBy navigation properties
            IEnumerable<PropertyInfo> auditNavProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(UserEf) &&
                            (p.Name == "CreatedBy" || p.Name == "UpdatedBy" || p.Name == "DeletedBy"));

            foreach (PropertyInfo navProperty in auditNavProperties)
                modelBuilder.Entity(entityType.ClrType).Ignore(navProperty.Name);
        }

        // Apply centralized snake_case naming convention (tables, columns, constraints, indexes)
        ModelBuilderExtensions.ApplySnakeCaseNamingConvention(modelBuilder);

        // User configuration
        modelBuilder.Entity<UserEf>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.IsDeleted).HasDefaultValue(false);
            
            // Pending 2FA fields configuration
            entity.Property(u => u.PendingTwoFactorSecret).HasMaxLength(128);
            entity.Property(u => u.PendingTwoFactorSecretExpiresAt);

            // Soft delete filter
            entity.HasQueryFilter(u => !u.IsDeleted);

            // Indexes - explicitly name them to ensure snake_case
            entity.HasIndex(u => u.Username).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_users_username");
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_users_email");
        });

        // UserDevice configuration
        modelBuilder.Entity<UserDeviceEf>(entity =>
        {
            entity.HasKey(ud => ud.Id);

            // Configure UUID generation
            entity.Property(ud => ud.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(ud => ud.UserId).IsRequired();
            entity.Property(ud => ud.DeviceId).IsRequired().HasMaxLength(255);
            entity.Property(ud => ud.DeviceName).IsRequired().HasMaxLength(200);
            entity.Property(ud => ud.DeviceType).IsRequired().HasMaxLength(50);
            entity.Property(ud => ud.UserAgent).HasMaxLength(500);
            entity.Property(ud => ud.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(ud => ud.Location).HasMaxLength(255);
            entity.Property(ud => ud.Browser).HasMaxLength(100);
            entity.Property(ud => ud.OperatingSystem).HasMaxLength(100);
            entity.Property(ud => ud.RefreshToken).HasMaxLength(500);
            entity.Property(ud => ud.ActiveAccessTokenJti).HasMaxLength(255);
            entity.Property(ud => ud.IsActive).HasDefaultValue(true);
            entity.Property(ud => ud.IsTrusted).HasDefaultValue(false);
            entity.Property(ud => ud.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(ud => !ud.IsDeleted);

            // Indexes - DeviceId should be unique per user
            entity.HasIndex(ud => new { ud.DeviceId, ud.UserId })
                .IsUnique()
                .HasFilter("is_deleted = false")
                .HasDatabaseName("ix_user_devices_device_user");
            entity.HasIndex(ud => ud.UserId).HasDatabaseName("ix_user_devices_user_id");
            entity.HasIndex(ud => new { ud.UserId, ud.IsActive }).HasDatabaseName("ix_user_devices_user_active");
            entity.HasIndex(ud => ud.LastLoginAt).HasDatabaseName("ix_user_devices_last_login");
            entity.HasIndex(ud => ud.CreatedAt).HasDatabaseName("ix_user_devices_created_at");

            // Relationships
            entity.HasOne(ud => ud.User)
                .WithMany(u => u.UserDevices)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserTheme configuration
        modelBuilder.Entity<UserThemeEf>(entity =>
        {
            entity.HasKey(ut => ut.Id);

            entity.Property(ut => ut.UserId).IsRequired();
            entity.Property(ut => ut.Theme).IsRequired().HasMaxLength(50);
            entity.Property(ut => ut.PrimaryColor).HasMaxLength(20);
            entity.Property(ut => ut.Transparency).HasPrecision(3, 2); // 0.00 to 1.00
            entity.Property(ut => ut.BorderRadius).IsRequired();
            entity.Property(ut => ut.DarkTheme).HasDefaultValue(false);
            entity.Property(ut => ut.PinNavbar).HasDefaultValue(false);
            entity.Property(ut => ut.CompactMode).HasDefaultValue(false);
            entity.Property(ut => ut.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(ut => !ut.IsDeleted);

            // Index - one theme per user
            entity.HasIndex(ut => ut.UserId)
                .IsUnique()
                .HasFilter("is_deleted = false")
                .HasDatabaseName("ix_user_themes_user_id");

            // Relationships
            entity.HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Authorization configurations
        modelBuilder.Entity<PermissionEf>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Resource).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Action).IsRequired().HasMaxLength(100);
            entity.Property(p => p.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.HasIndex(p => new { p.Resource, p.Action }).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_permissions_resource_action");
        });

        modelBuilder.Entity<RoleEf>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Code).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Rank).IsRequired();
            entity.Property(r => r.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(r => !r.IsDeleted);
            entity.HasIndex(r => r.Code).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_roles_code");
            entity.HasIndex(r => r.Rank).HasDatabaseName("ix_roles_rank");
        });

        modelBuilder.Entity<ResourceEf>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Type).IsRequired().HasMaxLength(100);
            entity.Property(r => r.NodeId).IsRequired();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(r => !r.IsDeleted);
            entity.HasIndex(r => new { r.Type, r.NodeId }).HasDatabaseName("ix_resources_type_node");
            entity.HasIndex(r => r.NodeId).HasDatabaseName("ix_resources_node_id");

            // Relationships
            entity.HasOne(r => r.Node)
                .WithMany(n => n.Resources)
                .HasForeignKey(r => r.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermissionEf>(entity =>
        {
            // Composite key
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.Property(rp => rp.RoleId).IsRequired();
            entity.Property(rp => rp.PermissionId).IsRequired();

            // Relationships
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNodeRoleEf>(entity =>
        {
            // Composite key
            entity.HasKey(unr => new { unr.UserId, unr.NodeId, unr.RoleId });

            entity.Property(unr => unr.UserId).IsRequired();
            entity.Property(unr => unr.NodeId).IsRequired();
            entity.Property(unr => unr.RoleId).IsRequired();

            // Indexes for query performance
            entity.HasIndex(unr => unr.UserId).HasDatabaseName("ix_user_node_roles_user_id");
            entity.HasIndex(unr => unr.NodeId).HasDatabaseName("ix_user_node_roles_node_id");
            entity.HasIndex(unr => unr.RoleId).HasDatabaseName("ix_user_node_roles_role_id");

            // Relationships
            entity.HasOne(unr => unr.User)
                .WithMany(u => u.UserNodeRoles)
                .HasForeignKey(unr => unr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(unr => unr.Node)
                .WithMany(n => n.UserNodeRoles)
                .HasForeignKey(unr => unr.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(unr => unr.Role)
                .WithMany(r => r.UserNodeRoles)
                .HasForeignKey(unr => unr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Organizations configurations
        modelBuilder.Entity<OrgNodeEf>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Type).IsRequired();
            entity.Property(n => n.Name).IsRequired().HasMaxLength(200);
            entity.Property(n => n.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(n => !n.IsDeleted);
            entity.HasIndex(n => n.ParentId).HasDatabaseName("ix_org_nodes_parent_id");
            entity.HasIndex(n => n.Type).HasDatabaseName("ix_org_nodes_type");

            // Self-referencing relationship
            entity.HasOne(n => n.Parent)
                .WithMany(n => n.Children)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompanyDetailsEf>(entity =>
        {
            entity.HasKey(cd => cd.Id);
            entity.Property(cd => cd.NodeId).IsRequired();
            entity.Property(cd => cd.TaxCode).HasMaxLength(20);
            entity.Property(cd => cd.Domain).HasMaxLength(255);
            entity.Property(cd => cd.Address).HasMaxLength(500);
            entity.Property(cd => cd.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(cd => !cd.IsDeleted);
            entity.HasIndex(cd => cd.NodeId).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_company_details_node_id");
            entity.HasIndex(cd => cd.TaxCode).IsUnique().HasFilter("is_deleted = false AND tax_code IS NOT NULL")
                .HasDatabaseName("ix_company_details_tax_code");
            entity.HasIndex(cd => cd.Domain).IsUnique().HasFilter("is_deleted = false AND domain IS NOT NULL")
                .HasDatabaseName("ix_company_details_domain");

            // Relationship
            entity.HasOne(cd => cd.Node)
                .WithOne(n => n.CompanyDetails)
                .HasForeignKey<CompanyDetailsEf>(cd => cd.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DepartmentDetailsEf>(entity =>
        {
            entity.HasKey(dd => dd.Id);
            entity.Property(dd => dd.NodeId).IsRequired();
            entity.Property(dd => dd.CostCenter).HasMaxLength(30);
            entity.Property(dd => dd.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(dd => !dd.IsDeleted);
            entity.HasIndex(dd => dd.NodeId).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_department_details_node_id");
            entity.HasIndex(dd => dd.CostCenter).IsUnique().HasFilter("is_deleted = false AND cost_center IS NOT NULL")
                .HasDatabaseName("ix_department_details_cost_center");

            // Relationship
            entity.HasOne(dd => dd.Node)
                .WithOne(n => n.DepartmentDetails)
                .HasForeignKey<DepartmentDetailsEf>(dd => dd.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Asset configurations
        modelBuilder.Entity<AssetEf>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(500);
            entity.Property(a => a.SerialNo).HasMaxLength(100);
            entity.Property(a => a.AssetTag).HasMaxLength(50);
            entity.Property(a => a.Barcode).HasMaxLength(100);
            entity.Property(a => a.QRCode).HasMaxLength(500);
            entity.Property(a => a.RFIDTag).HasMaxLength(100);
            entity.Property(a => a.PurchaseOrderNo).HasMaxLength(50);
            entity.Property(a => a.InvoiceNo).HasMaxLength(50);
            entity.Property(a => a.LocationCode).HasMaxLength(50);
            entity.Property(a => a.Notes).HasMaxLength(2000);
            entity.Property(a => a.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(a => !a.IsDeleted);

            // Indexes
            entity.HasIndex(a => a.SerialNo).IsUnique().HasFilter("is_deleted = false AND serial_no IS NOT NULL")
                .HasDatabaseName("ix_assets_serial_no");
            entity.HasIndex(a => a.AssetTag).IsUnique().HasFilter("is_deleted = false AND asset_tag IS NOT NULL")
                .HasDatabaseName("ix_assets_asset_tag");
            entity.HasIndex(a => a.Barcode).IsUnique().HasFilter("is_deleted = false AND barcode IS NOT NULL")
                .HasDatabaseName("ix_assets_barcode");
            entity.HasIndex(a => a.QRCode).IsUnique().HasFilter("is_deleted = false AND qrcode IS NOT NULL")
                .HasDatabaseName("ix_assets_qr_code");
            entity.HasIndex(a => a.RFIDTag).IsUnique().HasFilter("is_deleted = false AND rfidtag IS NOT NULL")
                .HasDatabaseName("ix_assets_rfid_tag");
            entity.HasIndex(a => a.CompanyId).HasDatabaseName("ix_assets_company_id");
            entity.HasIndex(a => a.AssetTypeId).HasDatabaseName("ix_assets_asset_type_id");
            entity.HasIndex(a => a.CategoryId).HasDatabaseName("ix_assets_category_id");
            entity.HasIndex(a => a.ModelId).HasDatabaseName("ix_assets_model_id");
            entity.HasIndex(a => a.ManufacturerId).HasDatabaseName("ix_assets_manufacturer_id");
            entity.HasIndex(a => a.ConditionId).HasDatabaseName("ix_assets_condition_id");
            entity.HasIndex(a => a.LocationId).HasDatabaseName("ix_assets_location_id");
            entity.HasIndex(a => a.CountryId).HasDatabaseName("ix_assets_country_id");
            entity.HasIndex(a => a.OwnerId).HasDatabaseName("ix_assets_owner_id");
            entity.HasIndex(a => a.UsageStatusId).HasDatabaseName("ix_assets_usage_status_id");
            entity.HasIndex(a => a.LifecycleStatusId).HasDatabaseName("ix_assets_lifecycle_status_id");
            entity.HasIndex(a => a.SupplierId).HasDatabaseName("ix_assets_supplier_id");

            // Relationships
            entity.HasOne(a => a.Company).WithMany().HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.AssetType).WithMany(at => at.Assets).HasForeignKey(a => a.AssetTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Category).WithMany(c => c.Assets).HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Model).WithMany(m => m.Assets).HasForeignKey(a => a.ModelId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Manufacturer).WithMany(m => m.Assets).HasForeignKey(a => a.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Condition).WithMany(c => c.Assets).HasForeignKey(a => a.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Location).WithMany(l => l.Assets).HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Country).WithMany().HasForeignKey(a => a.CountryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Owner).WithMany().HasForeignKey(a => a.OwnerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.UsageStatus).WithMany(us => us.Assets).HasForeignKey(a => a.UsageStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.LifecycleStatus).WithMany(ls => ls.Assets).HasForeignKey(a => a.LifecycleStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Supplier).WithMany(s => s.Assets).HasForeignKey(a => a.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.CreatedBy).WithMany().HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.UpdatedBy).WithMany().HasForeignKey(a => a.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssetEventEf>(entity =>
        {
            entity.HasKey(ae => ae.Id);
            entity.Property(ae => ae.EventCode).IsRequired().HasMaxLength(50);
            entity.Property(ae => ae.FromLifecycleCode).HasMaxLength(50);
            entity.Property(ae => ae.ToLifecycleCode).HasMaxLength(50);
            entity.Property(ae => ae.Note).HasMaxLength(1000);
            entity.Property(ae => ae.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(ae => !ae.IsDeleted);

            // Indexes
            entity.HasIndex(ae => ae.AssetId).HasDatabaseName("ix_asset_events_asset_id");
            entity.HasIndex(ae => ae.EventTypeId).HasDatabaseName("ix_asset_events_event_type_id");
            entity.HasIndex(ae => ae.ActorId).HasDatabaseName("ix_asset_events_actor_id");
            entity.HasIndex(ae => ae.At).HasDatabaseName("ix_asset_events_at");

            // Relationships
            entity.HasOne(ae => ae.Asset).WithMany(a => a.AssetEvents).HasForeignKey(ae => ae.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ae => ae.EventType).WithMany(aet => aet.AssetEvents).HasForeignKey(ae => ae.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ae => ae.Actor).WithMany().HasForeignKey(ae => ae.ActorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssignmentEf>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AssigneeType).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Comments).HasMaxLength(1000);
            entity.Property(a => a.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(a => !a.IsDeleted);

            // Indexes
            entity.HasIndex(a => a.AssetId).HasDatabaseName("ix_assignments_asset_id");
            entity.HasIndex(a => a.AssigneeId).HasDatabaseName("ix_assignments_assignee_id");
            entity.HasIndex(a => a.ByUserId).HasDatabaseName("ix_assignments_by_user_id");
            entity.HasIndex(a => a.AssignedAt).HasDatabaseName("ix_assignments_assigned_at");
            entity.HasIndex(a => a.ReleasedAt).HasDatabaseName("ix_assignments_released_at");

            // Relationships
            entity.HasOne(a => a.Asset).WithMany(a => a.Assignments).HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.ByUser).WithMany().HasForeignKey(a => a.ByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttachmentEf>(entity =>
        {
            entity.HasKey(att => att.Id);
            entity.Property(att => att.FileName).HasMaxLength(255);
            entity.Property(att => att.FileUrl).HasMaxLength(1000);
            entity.Property(att => att.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(att => !att.IsDeleted);

            // Indexes
            entity.HasIndex(att => att.AssetId).HasDatabaseName("ix_attachments_asset_id");
            entity.HasIndex(att => att.UploadedBy).HasDatabaseName("ix_attachments_uploaded_by");
            entity.HasIndex(att => att.UploadedAt).HasDatabaseName("ix_attachments_uploaded_at");

            // Relationships
            entity.HasOne(att => att.Asset).WithMany(a => a.Attachments).HasForeignKey(att => att.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(att => att.Uploader).WithMany().HasForeignKey(att => att.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinanceEntryEf>(entity =>
        {
            entity.HasKey(fe => fe.Id);
            entity.Property(fe => fe.EntryType).IsRequired().HasMaxLength(50);
            entity.Property(fe => fe.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(fe => !fe.IsDeleted);

            // Indexes
            entity.HasIndex(fe => fe.AssetId).HasDatabaseName("ix_finance_entries_asset_id");
            entity.HasIndex(fe => fe.Period).HasDatabaseName("ix_finance_entries_period");
            entity.HasIndex(fe => fe.EntryType).HasDatabaseName("ix_finance_entries_entry_type");

            // Relationships
            entity.HasOne(fe => fe.Asset).WithMany(a => a.FinanceEntries).HasForeignKey(fe => fe.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(fe => fe.CreatedBy).WithMany().HasForeignKey(fe => fe.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Categories & Types configurations
        modelBuilder.Entity<AssetCategoryEf>(entity =>
        {
            entity.HasKey(ac => ac.Id);
            entity.Property(ac => ac.Name).IsRequired().HasMaxLength(200);
            entity.Property(ac => ac.Code).HasMaxLength(50);
            entity.Property(ac => ac.Description).HasMaxLength(500);
            entity.Property(ac => ac.LongDescription).HasMaxLength(2000);
            entity.Property(ac => ac.Path).HasMaxLength(1000);
            entity.Property(ac => ac.CategoryType).HasMaxLength(100);
            entity.Property(ac => ac.Industry).HasMaxLength(100);
            entity.Property(ac => ac.Sector).HasMaxLength(100);
            entity.Property(ac => ac.GLAccountCode).HasMaxLength(50);
            entity.Property(ac => ac.DepreciationAccountCode).HasMaxLength(50);
            entity.Property(ac => ac.CostCenter).HasMaxLength(50);
            entity.Property(ac => ac.DefaultDepreciationMethod).HasMaxLength(100);
            entity.Property(ac => ac.ValuationMethod).HasMaxLength(100);
            entity.Property(ac => ac.ComplianceStandards).HasColumnType("text");
            entity.Property(ac => ac.Tags).HasColumnType("text");
            entity.Property(ac => ac.SearchKeywords).HasColumnType("text");
            entity.Property(ac => ac.Aliases).HasColumnType("text");
            entity.Property(ac => ac.InternalNotes).HasColumnType("text");
            entity.Property(ac => ac.IconName).HasMaxLength(100);
            entity.Property(ac => ac.IconUrl).HasMaxLength(500);
            entity.Property(ac => ac.Color).HasMaxLength(20);
            entity.Property(ac => ac.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(ac => !ac.IsDeleted);

            // Indexes
            entity.HasIndex(ac => ac.Code).IsUnique().HasFilter("is_deleted = false AND code IS NOT NULL")
                .HasDatabaseName("ix_asset_categories_code");
            entity.HasIndex(ac => ac.Name).HasDatabaseName("ix_asset_categories_name");
            entity.HasIndex(ac => ac.ParentId).HasDatabaseName("ix_asset_categories_parent_id");
            entity.HasIndex(ac => ac.Level).HasDatabaseName("ix_asset_categories_level");
            entity.HasIndex(ac => ac.IsActive).HasDatabaseName("ix_asset_categories_is_active");

            // Self-referencing relationship
            entity.HasOne(ac => ac.Parent)
                .WithMany(ac => ac.Children)
                .HasForeignKey(ac => ac.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssetTypeEf>(entity =>
        {
            entity.HasKey(at => at.Id);
            entity.Property(at => at.Code).IsRequired().HasMaxLength(20);
            entity.Property(at => at.Name).IsRequired().HasMaxLength(200);
            entity.Property(at => at.Description).HasMaxLength(500);
            entity.Property(at => at.LongDescription).HasMaxLength(2000);
            entity.Property(at => at.Path).HasMaxLength(1000);
            entity.Property(at => at.AssetClass).HasMaxLength(100);
            entity.Property(at => at.Category).HasMaxLength(100);
            entity.Property(at => at.Subcategory).HasMaxLength(100);
            entity.Property(at => at.DefaultDepreciationMethod).HasMaxLength(100);
            entity.Property(at => at.DepreciationAccountCode).HasMaxLength(50);
            entity.Property(at => at.AccumulatedDepreciationAccountCode).HasMaxLength(50);
            entity.Property(at => at.GLAccountCode).HasMaxLength(50);
            entity.Property(at => at.AssetAccountCode).HasMaxLength(50);
            entity.Property(at => at.ExpenseAccountCode).HasMaxLength(50);
            entity.Property(at => at.CostCenter).HasMaxLength(50);
            entity.Property(at => at.DefaultMaintenanceType).HasMaxLength(100);
            entity.Property(at => at.ValuationCurrency).HasMaxLength(3);
            entity.Property(at => at.ValuationMethod).HasMaxLength(100);
            entity.Property(at => at.ComplianceStandards).HasColumnType("text");
            entity.Property(at => at.RegulatoryRequirements).HasColumnType("text");
            entity.Property(at => at.DefaultSecurityClassification).HasMaxLength(50);
            entity.Property(at => at.ApprovalWorkflow).HasColumnType("text");
            entity.Property(at => at.CustomFieldsSchema).HasColumnType("text");
            entity.Property(at => at.RequiredFields).HasColumnType("text");
            entity.Property(at => at.Tags).HasColumnType("text");
            entity.Property(at => at.SearchKeywords).HasColumnType("text");
            entity.Property(at => at.Aliases).HasColumnType("text");
            entity.Property(at => at.InternalNotes).HasColumnType("text");
            entity.Property(at => at.ProcurementNotes).HasColumnType("text");
            entity.Property(at => at.IconName).HasMaxLength(100);
            entity.Property(at => at.IconUrl).HasMaxLength(500);
            entity.Property(at => at.Color).HasMaxLength(20);
            entity.Property(at => at.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(at => !at.IsDeleted);

            // Indexes
            entity.HasIndex(at => at.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_asset_types_code");
            entity.HasIndex(at => at.Name).HasDatabaseName("ix_asset_types_name");
            entity.HasIndex(at => at.ParentId).HasDatabaseName("ix_asset_types_parent_id");
            entity.HasIndex(at => at.Level).HasDatabaseName("ix_asset_types_level");
            entity.HasIndex(at => at.IsActive).HasDatabaseName("ix_asset_types_is_active");

            // Self-referencing relationship
            entity.HasOne(at => at.Parent)
                .WithMany(at => at.Children)
                .HasForeignKey(at => at.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Status configurations
        modelBuilder.Entity<UsageStatusEf>(entity =>
        {
            entity.HasKey(us => us.Id);
            entity.Property(us => us.Code).IsRequired().HasMaxLength(20);
            entity.Property(us => us.Name).IsRequired().HasMaxLength(100);
            entity.Property(us => us.Description).HasMaxLength(500);
            entity.Property(us => us.Color).HasMaxLength(20);
            entity.Property(us => us.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(us => !us.IsDeleted);
            entity.HasIndex(us => us.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_usage_statuses_code");
        });

        modelBuilder.Entity<LifecycleStatusEf>(entity =>
        {
            entity.HasKey(ls => ls.Id);
            entity.Property(ls => ls.Code).IsRequired().HasMaxLength(20);
            entity.Property(ls => ls.Name).IsRequired().HasMaxLength(100);
            entity.Property(ls => ls.Description).HasMaxLength(500);
            entity.Property(ls => ls.Color).HasMaxLength(20);
            entity.Property(ls => ls.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(ls => !ls.IsDeleted);
            entity.HasIndex(ls => ls.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_lifecycle_statuses_code");
        });

        modelBuilder.Entity<AssetEventTypeEf>(entity =>
        {
            entity.HasKey(aet => aet.Id);
            entity.Property(aet => aet.Code).IsRequired().HasMaxLength(20);
            entity.Property(aet => aet.Name).IsRequired().HasMaxLength(100);
            entity.Property(aet => aet.Description).HasMaxLength(500);
            entity.Property(aet => aet.Color).HasMaxLength(20);
            entity.Property(aet => aet.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(aet => !aet.IsDeleted);
            entity.HasIndex(aet => aet.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_asset_event_types_code");
        });

        modelBuilder.Entity<AssetConditionEf>(entity =>
        {
            entity.HasKey(ac => ac.Id);
            entity.Property(ac => ac.Name).IsRequired().HasMaxLength(100);
            entity.Property(ac => ac.Description).HasMaxLength(500);
            entity.Property(ac => ac.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(ac => !ac.IsDeleted);
            entity.HasIndex(ac => ac.Name).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_asset_conditions_name");
        });

        // Geography & Location configurations
        modelBuilder.Entity<CountryEf>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Code).IsRequired().HasMaxLength(3);
            entity.Property(c => c.Alpha3Code).HasMaxLength(3);
            entity.Property(c => c.NumericCode).HasMaxLength(3);
            entity.Property(c => c.Capital).HasMaxLength(100);
            entity.Property(c => c.Region).HasMaxLength(50);
            entity.Property(c => c.SubRegion).HasMaxLength(50);
            entity.Property(c => c.CurrencyCode).HasMaxLength(3);
            entity.Property(c => c.CurrencyName).HasMaxLength(50);
            entity.Property(c => c.CallingCode).HasMaxLength(10);
            entity.Property(c => c.TopLevelDomain).HasMaxLength(5);
            entity.Property(c => c.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(c => !c.IsDeleted);

            // Indexes
            entity.HasIndex(c => c.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_countries_code");
            entity.HasIndex(c => c.Alpha3Code).IsUnique().HasFilter("is_deleted = false AND alpha3_code IS NOT NULL")
                .HasDatabaseName("ix_countries_alpha3_code");
            entity.HasIndex(c => c.Name).HasDatabaseName("ix_countries_name");
        });

        modelBuilder.Entity<LocationEf>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Code).HasMaxLength(50);
            entity.Property(l => l.Type).HasMaxLength(50);
            entity.Property(l => l.FullPath).HasMaxLength(1000);
            entity.Property(l => l.PathIds).HasMaxLength(1000);
            entity.Property(l => l.Description).HasMaxLength(500);
            entity.Property(l => l.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(l => !l.IsDeleted);

            // Indexes
            entity.HasIndex(l => l.Code).IsUnique().HasFilter("is_deleted = false AND code IS NOT NULL")
                .HasDatabaseName("ix_locations_code");
            entity.HasIndex(l => l.Name).HasDatabaseName("ix_locations_name");
            entity.HasIndex(l => l.ParentId).HasDatabaseName("ix_locations_parent_id");
            entity.HasIndex(l => l.CompanyId).HasDatabaseName("ix_locations_company_id");
            entity.HasIndex(l => l.CountryId).HasDatabaseName("ix_locations_country_id");

            // Relationships
            entity.HasOne(l => l.Company).WithMany().HasForeignKey(l => l.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.Country).WithMany().HasForeignKey(l => l.CountryId).OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship
            entity.HasOne(l => l.Parent)
                .WithMany(l => l.Children)
                .HasForeignKey(l => l.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Manufacturers & Models configurations
        modelBuilder.Entity<ManufacturerEf>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Code).HasMaxLength(50);
            entity.Property(m => m.Description).HasMaxLength(1000);
            entity.Property(m => m.Website).HasMaxLength(500);
            entity.Property(m => m.LogoUrl).HasMaxLength(500);
            entity.Property(m => m.HeadquartersAddress).HasMaxLength(500);
            entity.Property(m => m.ContactEmail).HasMaxLength(255);
            entity.Property(m => m.ContactPhone).HasMaxLength(50);
            entity.Property(m => m.TaxId).HasMaxLength(50);
            entity.Property(m => m.RegistrationNumber).HasMaxLength(50);
            entity.Property(m => m.Industry).HasMaxLength(100);
            entity.Property(m => m.Certifications).HasColumnType("text");
            entity.Property(m => m.WarrantyTerms).HasColumnType("text");
            entity.Property(m => m.SupportContact).HasMaxLength(500);
            entity.Property(m => m.Notes).HasColumnType("text");
            entity.Property(m => m.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(m => !m.IsDeleted);

            // Indexes
            entity.HasIndex(m => m.Code).IsUnique().HasFilter("is_deleted = false AND code IS NOT NULL")
                .HasDatabaseName("ix_manufacturers_code");
            entity.HasIndex(m => m.Name).HasDatabaseName("ix_manufacturers_name");
            entity.HasIndex(m => m.CountryId).HasDatabaseName("ix_manufacturers_country_id");
            entity.HasIndex(m => m.IsActive).HasDatabaseName("ix_manufacturers_is_active");

            // Relationships
            entity.HasOne(m => m.Country).WithMany().HasForeignKey(m => m.CountryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModelEf>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(300);
            entity.Property(m => m.ModelNumber).HasMaxLength(100);
            entity.Property(m => m.SKU).HasMaxLength(100);
            entity.Property(m => m.PartNumber).HasMaxLength(100);
            entity.Property(m => m.Description).HasMaxLength(1000);
            entity.Property(m => m.ProductFamily).HasMaxLength(100);
            entity.Property(m => m.Generation).HasMaxLength(50);
            entity.Property(m => m.Series).HasMaxLength(100);
            entity.Property(m => m.LifecycleStatus).HasMaxLength(50);
            entity.Property(m => m.Processor).HasMaxLength(200);
            entity.Property(m => m.Memory).HasMaxLength(200);
            entity.Property(m => m.Storage).HasMaxLength(200);
            entity.Property(m => m.Display).HasMaxLength(200);
            entity.Property(m => m.Graphics).HasMaxLength(200);
            entity.Property(m => m.OperatingSystem).HasMaxLength(100);
            entity.Property(m => m.Dimensions).HasMaxLength(100);
            entity.Property(m => m.DimensionUnit).HasMaxLength(10);
            entity.Property(m => m.Color).HasMaxLength(50);
            entity.Property(m => m.Material).HasMaxLength(200);
            entity.Property(m => m.PowerRequirements).HasMaxLength(200);
            entity.Property(m => m.EnergyRating).HasMaxLength(50);
            entity.Property(m => m.WarrantyType).HasMaxLength(100);
            entity.Property(m => m.Currency).HasMaxLength(3);
            entity.Property(m => m.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(m => !m.IsDeleted);

            // Indexes
            entity.HasIndex(m => m.ModelNumber).IsUnique().HasFilter("is_deleted = false AND model_number IS NOT NULL")
                .HasDatabaseName("ix_models_model_number");
            entity.HasIndex(m => m.SKU).IsUnique().HasFilter("is_deleted = false AND sku IS NOT NULL")
                .HasDatabaseName("ix_models_sku");
            entity.HasIndex(m => m.Name).HasDatabaseName("ix_models_name");
            entity.HasIndex(m => m.ManufacturerId).HasDatabaseName("ix_models_manufacturer_id");
            entity.HasIndex(m => m.CategoryId).HasDatabaseName("ix_models_category_id");
            entity.HasIndex(m => m.TypeId).HasDatabaseName("ix_models_type_id");
            entity.HasIndex(m => m.IsActive).HasDatabaseName("ix_models_is_active");

            // Relationships
            entity.HasOne(m => m.Manufacturer).WithMany(m => m.Models).HasForeignKey(m => m.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Category).WithMany(c => c.Models).HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Type).WithMany(at => at.Models).HasForeignKey(m => m.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Suppliers configuration
        modelBuilder.Entity<SupplierEf>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Code).HasMaxLength(50);
            entity.Property(s => s.Description).HasMaxLength(1000);
            entity.Property(s => s.Website).HasMaxLength(500);
            entity.Property(s => s.LogoUrl).HasMaxLength(500);
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.ContactEmail).HasMaxLength(255);
            entity.Property(s => s.ContactPhone).HasMaxLength(50);
            entity.Property(s => s.TaxId).HasMaxLength(50);
            entity.Property(s => s.RegistrationNumber).HasMaxLength(50);
            entity.Property(s => s.Industry).HasMaxLength(100);
            entity.Property(s => s.PaymentTerms).HasMaxLength(200);
            entity.Property(s => s.ContractStartDate).HasColumnType("date");
            entity.Property(s => s.ContractEndDate).HasColumnType("date");
            entity.Property(s => s.Notes).HasColumnType("text");
            entity.Property(s => s.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(s => !s.IsDeleted);

            // Indexes
            entity.HasIndex(s => s.Code).IsUnique().HasFilter("is_deleted = false AND code IS NOT NULL")
                .HasDatabaseName("ix_suppliers_code");
            entity.HasIndex(s => s.Name).HasDatabaseName("ix_suppliers_name");
            entity.HasIndex(s => s.ContactEmail).IsUnique()
                .HasFilter("is_deleted = false AND contact_email IS NOT NULL")
                .HasDatabaseName("ix_suppliers_contact_email");
            entity.HasIndex(s => s.CountryId).HasDatabaseName("ix_suppliers_country_id");
            entity.HasIndex(s => s.IsActive).HasDatabaseName("ix_suppliers_is_active");

            // Relationships
            entity.HasOne(s => s.Country).WithMany().HasForeignKey(s => s.CountryId).OnDelete(DeleteBehavior.Restrict);
        });

        // SigningKey configuration (for JWKS)
        modelBuilder.Entity<SigningKeyEf>(entity =>
        {
            entity.HasKey(sk => sk.Id);
            entity.Property(sk => sk.KeyId).IsRequired().HasMaxLength(100);
            entity.Property(sk => sk.PublicKey).IsRequired();
            entity.Property(sk => sk.PrivateKey).IsRequired();
            entity.Property(sk => sk.Algorithm).IsRequired().HasMaxLength(20);
            entity.Property(sk => sk.KeySize).IsRequired();
            entity.Property(sk => sk.Use).IsRequired().HasMaxLength(10);
            entity.Property(sk => sk.KeyType).IsRequired().HasMaxLength(10);
            entity.Property(sk => sk.IsActive).HasDefaultValue(true);
            entity.Property(sk => sk.IsRevoked).HasDefaultValue(false);
            entity.Property(sk => sk.RevocationReason).HasMaxLength(500);
            entity.Property(sk => sk.Description).HasMaxLength(500);
            entity.Property(sk => sk.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(sk => !sk.IsDeleted);

            // Indexes
            entity.HasIndex(sk => sk.KeyId).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_signing_keys_key_id");
            entity.HasIndex(sk => sk.IsActive).HasDatabaseName("ix_signing_keys_is_active");
            entity.HasIndex(sk => sk.IsRevoked).HasDatabaseName("ix_signing_keys_is_revoked");
            entity.HasIndex(sk => sk.ExpiresAt).HasDatabaseName("ix_signing_keys_expires_at");
        });

        // MenuItem configuration
        modelBuilder.Entity<MenuItemEf>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Code).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(500);
            entity.Property(m => m.Icon).HasMaxLength(100);
            entity.Property(m => m.Route).HasMaxLength(500);
            entity.Property(m => m.ExternalUrl).HasMaxLength(1000);
            entity.Property(m => m.RequiredPermission).HasMaxLength(100);
            entity.Property(m => m.RequiredRoles).HasMaxLength(500);
            entity.Property(m => m.CssClass).HasMaxLength(200);
            entity.Property(m => m.Badge).HasMaxLength(50);
            entity.Property(m => m.BadgeVariant).HasMaxLength(50);
            entity.Property(m => m.IsVisible).HasDefaultValue(true);
            entity.Property(m => m.IsEnabled).HasDefaultValue(true);
            entity.Property(m => m.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(m => !m.IsDeleted);

            // Self-referencing relationship for parent-child
            entity.HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(m => m.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_menu_items_code");
            entity.HasIndex(m => m.ParentId).HasDatabaseName("ix_menu_items_parent_id");
            entity.HasIndex(m => m.SortOrder).HasDatabaseName("ix_menu_items_sort_order");
            entity.HasIndex(m => m.IsVisible).HasDatabaseName("ix_menu_items_is_visible");
        });

        // SystemSetting configuration
        modelBuilder.Entity<SystemSettingEf>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Key).IsRequired().HasMaxLength(200);
            entity.Property(s => s.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Group).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.IsVisible).HasDefaultValue(true);
            entity.Property(s => s.IsEditable).HasDefaultValue(true);
            entity.Property(s => s.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(s => !s.IsDeleted);

            // Indexes
            entity.HasIndex(s => s.Key).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_system_settings_key");
            entity.HasIndex(s => s.Group).HasDatabaseName("ix_system_settings_group");
            entity.HasIndex(s => s.IsVisible).HasDatabaseName("ix_system_settings_is_visible");
        });

        // EmailTemplate configuration
        modelBuilder.Entity<EmailTemplateEf>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.HtmlBody).IsRequired();
            entity.Property(e => e.PlainTextBody);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.AvailablePlaceholders);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsSystem).HasDefaultValue(false);
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasQueryFilter(e => !e.IsDeleted);

            // Indexes
            entity.HasIndex(e => e.Code).IsUnique().HasFilter("is_deleted = false")
                .HasDatabaseName("ix_email_templates_code");
            entity.HasIndex(e => e.Category).HasDatabaseName("ix_email_templates_category");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("ix_email_templates_is_active");
            entity.HasIndex(e => e.IsSystem).HasDatabaseName("ix_email_templates_is_system");
        });
    }
}
