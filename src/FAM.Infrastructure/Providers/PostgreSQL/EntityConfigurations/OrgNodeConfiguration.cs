using FAM.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class OrgNodeConfiguration : IEntityTypeConfiguration<OrgNode>
{
    public void Configure(EntityTypeBuilder<OrgNode> entity)
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
    }
}
