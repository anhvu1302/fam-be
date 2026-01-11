using FAM.Domain.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> entity)
    {
        entity.HasKey(r => r.Id);

        entity.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(100);

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
    }
}
