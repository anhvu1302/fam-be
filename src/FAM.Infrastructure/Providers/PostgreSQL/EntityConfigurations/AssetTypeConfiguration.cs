using FAM.Domain.Types;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssetTypeConfiguration : IEntityTypeConfiguration<AssetType>
{
    public void Configure(EntityTypeBuilder<AssetType> entity)
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
    }
}
