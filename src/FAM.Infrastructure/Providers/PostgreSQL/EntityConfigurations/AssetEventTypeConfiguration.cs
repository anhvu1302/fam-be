using FAM.Domain.Statuses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssetEventTypeConfiguration : IEntityTypeConfiguration<AssetEventType>
{
    public void Configure(EntityTypeBuilder<AssetEventType> entity)
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
    }
}
