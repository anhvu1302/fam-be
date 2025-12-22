using FAM.Domain.Conditions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssetConditionConfiguration : IEntityTypeConfiguration<AssetCondition>
{
    public void Configure(EntityTypeBuilder<AssetCondition> entity)
    {
        entity.HasKey(ac => ac.Id);
        entity.Property(ac => ac.Name).IsRequired().HasMaxLength(100);
        entity.Property(ac => ac.Description).HasMaxLength(500);
        entity.Property(ac => ac.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(ac => !ac.IsDeleted);
        entity.HasIndex(ac => ac.Name).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_asset_conditions_name");
    }
}
