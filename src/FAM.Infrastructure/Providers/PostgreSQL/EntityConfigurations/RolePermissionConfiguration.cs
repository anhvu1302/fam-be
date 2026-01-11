using FAM.Domain.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        // Composite key
        entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        entity.Property("RoleId").IsRequired();
        entity.Property("PermissionId").IsRequired();

        // Relationships
        entity.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey("RoleId")
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey("PermissionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
