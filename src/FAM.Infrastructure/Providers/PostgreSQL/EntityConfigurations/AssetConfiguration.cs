using FAM.Domain.Assets;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> entity)
    {
        entity.HasKey(a => a.Id);
        entity.Property(a => a.Name).IsRequired().HasMaxLength(500);

        // Value object conversions
        entity.Property(a => a.SerialNo).HasMaxLength(100);

        entity.Property(a => a.AssetTag).HasMaxLength(50);

        entity.Property(a => a.Barcode).HasMaxLength(100);
        entity.Property(a => a.QRCode).HasMaxLength(500);
        entity.Property(a => a.RFIDTag).HasMaxLength(100);
        entity.Property(a => a.PurchaseOrderNo).HasMaxLength(50);
        entity.Property(a => a.InvoiceNo).HasMaxLength(50);
        entity.Property(a => a.LocationCode).HasMaxLength(50);
        entity.Property(a => a.Notes).HasMaxLength(2000);

        // IT/Software Specific - Value objects as owned entities
        entity.OwnsOne(a => a.IPAddress, ip =>
        {
            ip.Property(i => i.Value).HasColumnName("ip_address").HasMaxLength(45);
            ip.Property(i => i.Type).HasColumnName("ip_address_type").HasMaxLength(10);
        });

        entity.OwnsOne(a => a.MACAddress, mac =>
        {
            mac.Property(m => m.Value).HasColumnName("mac_address").HasMaxLength(17);
            mac.Property(m => m.Format).HasColumnName("mac_address_format").HasMaxLength(20);
        });

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
        entity.HasIndex(a => a.SupplierId).HasDatabaseName("ix_assets_supplier_id");

        // Relationships
        entity.HasOne(a => a.Company).WithMany().HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.AssetType).WithMany().HasForeignKey(a => a.AssetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Category).WithMany().HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Model).WithMany(m => m.Assets).HasForeignKey(a => a.ModelId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Manufacturer).WithMany().HasForeignKey(a => a.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Condition).WithMany().HasForeignKey(a => a.ConditionId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Location).WithMany().HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Country).WithMany().HasForeignKey(a => a.CountryId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Owner).WithMany().HasForeignKey(a => a.OwnerId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Supplier).WithMany().HasForeignKey(a => a.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
