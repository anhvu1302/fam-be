using FAM.Domain.Common.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> entity)
    {
        entity.HasKey(m => m.Id);
        entity.Property(m => m.Code).IsRequired().HasMaxLength(100);
        entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
        entity.Property(m => m.Description).HasMaxLength(500);
        entity.Property(m => m.Icon).HasMaxLength(100);
        entity.Property(m => m.Route).HasMaxLength(500);
        entity.Property(m => m.ExternalUrl).HasMaxLength(1000);
        entity.Property(m => m.RequiredPermission).HasMaxLength(100);
        entity.Property(m => m.RequiredRoles).HasMaxLength(500);
        entity.Property(m => m.CssClass).HasMaxLength(200);
        entity.Property(m => m.Badge).HasMaxLength(50);
        entity.Property(m => m.BadgeVariant).HasMaxLength(50);
        entity.Property(m => m.IsVisible).HasDefaultValue(true);
        entity.Property(m => m.IsEnabled).HasDefaultValue(true);
        entity.Property(m => m.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(m => !m.IsDeleted);

        // Self-referencing relationship for parent-child
        entity.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        entity.HasIndex(m => m.Code).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_menu_items_code");
        entity.HasIndex(m => m.ParentId).HasDatabaseName("ix_menu_items_parent_id");
        entity.HasIndex(m => m.SortOrder).HasDatabaseName("ix_menu_items_sort_order");
        entity.HasIndex(m => m.IsVisible).HasDatabaseName("ix_menu_items_is_visible");
    }
}
