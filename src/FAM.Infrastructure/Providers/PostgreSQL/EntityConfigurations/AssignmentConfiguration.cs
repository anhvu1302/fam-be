using FAM.Domain.Assets;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> entity)
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
    }
}
