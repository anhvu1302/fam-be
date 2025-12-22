using FAM.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class UserNodeRoleConfiguration : IEntityTypeConfiguration<UserNodeRole>
{
    public void Configure(EntityTypeBuilder<UserNodeRole> entity)
    {
        // Composite key
        entity.HasKey(unr => new { unr.UserId, unr.NodeId, unr.RoleId });

        entity.Property("UserId").IsRequired();
        entity.Property("NodeId").IsRequired();
        entity.Property("RoleId").IsRequired();

        // Indexes for query performance
        entity.HasIndex("UserId").HasDatabaseName("ix_user_node_roles_user_id");
        entity.HasIndex("NodeId").HasDatabaseName("ix_user_node_roles_node_id");
        entity.HasIndex("RoleId").HasDatabaseName("ix_user_node_roles_role_id");

        // Relationships
        entity.HasOne(unr => unr.User)
            .WithMany(u => u.UserNodeRoles)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(unr => unr.Node)
            .WithMany(n => n.UserNodeRoles)
            .HasForeignKey("NodeId")
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(unr => unr.Role)
            .WithMany(r => r.UserNodeRoles)
            .HasForeignKey("RoleId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
