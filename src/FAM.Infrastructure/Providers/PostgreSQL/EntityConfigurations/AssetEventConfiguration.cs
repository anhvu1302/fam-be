using FAM.Domain.Assets;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssetEventConfiguration : IEntityTypeConfiguration<AssetEvent>
{
    public void Configure(EntityTypeBuilder<AssetEvent> entity)
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
        entity.HasIndex(ae => ae.ActorId).HasDatabaseName("ix_asset_events_actor_id");
        entity.HasIndex(ae => ae.At).HasDatabaseName("ix_asset_events_at");

        // Relationships
        entity.HasOne(ae => ae.Asset).WithMany(a => a.AssetEvents).HasForeignKey(ae => ae.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(ae => ae.Actor).WithMany().HasForeignKey(ae => ae.ActorId).OnDelete(DeleteBehavior.Restrict);
    }
}
