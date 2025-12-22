using FAM.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class DepartmentDetailsConfiguration : IEntityTypeConfiguration<DepartmentDetails>
{
    public void Configure(EntityTypeBuilder<DepartmentDetails> entity)
    {
        entity.HasKey(dd => dd.Id);
        entity.Property(dd => dd.NodeId).IsRequired();

        // Value object conversion
        entity.Property(dd => dd.CostCenter).HasMaxLength(50);

        entity.Property(dd => dd.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(dd => !dd.IsDeleted);
        entity.HasIndex(dd => dd.NodeId).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_department_details_node_id");
        entity.HasIndex(dd => dd.CostCenter).IsUnique().HasFilter("is_deleted = false AND cost_center IS NOT NULL")
            .HasDatabaseName("ix_department_details_cost_center");

        // Relationship
        entity.HasOne(dd => dd.Node)
            .WithOne(n => n.DepartmentDetails)
            .HasForeignKey<DepartmentDetails>(dd => dd.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
