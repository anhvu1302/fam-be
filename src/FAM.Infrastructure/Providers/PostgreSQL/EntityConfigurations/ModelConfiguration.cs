using FAM.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> entity)
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
        entity.Property(m => m.MSRPCurrency).HasMaxLength(3);
        entity.Property(m => m.CostCurrency).HasMaxLength(3);

        // Value object conversions for URLs
        entity.Property(m => m.SupportDocumentUrl).HasMaxLength(500);

        entity.Property(m => m.UserManualUrl).HasMaxLength(500);

        entity.Property(m => m.QuickStartGuideUrl).HasMaxLength(500);

        entity.Property(m => m.ImageUrl).HasMaxLength(500);

        entity.Property(m => m.ThumbnailUrl).HasMaxLength(500);

        entity.Property(m => m.ProductPageUrl).HasMaxLength(500);

        entity.Property(m => m.DatasheetUrl).HasMaxLength(500);

        entity.Property(m => m.VideoUrl).HasMaxLength(500);

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
    }
}
