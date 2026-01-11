using FAM.Domain.Categories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> entity)
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
    }
}
