using FAM.Domain.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("permissions");

        entity.HasKey(p => p.Id);

        // Map properties to columns
        entity.Property(p => p.Resource)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(100);

        // Audit fields
        entity.Property(p => p.CreatedAt);
        entity.Property(p => p.CreatedById);
        entity.Property(p => p.UpdatedAt);
        entity.Property(p => p.UpdatedById);
        entity.Property(p => p.IsDeleted).HasDefaultValue(false);
        entity.Property(p => p.DeletedAt);
        entity.Property(p => p.DeletedById);

        entity.HasQueryFilter(p => !p.IsDeleted);

        entity.HasIndex(p => new { p.Resource, p.Action })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_permissions_resource_action");
    }
}
