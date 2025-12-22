using FAM.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> entity)
    {
        entity.HasKey(s => s.Id);
        entity.Property(s => s.Key).IsRequired().HasMaxLength(200);
        entity.Property(s => s.DisplayName).IsRequired().HasMaxLength(200);
        entity.Property(s => s.Group).IsRequired().HasMaxLength(100);
        entity.Property(s => s.Description).HasMaxLength(500);
        entity.Property(s => s.IsVisible).HasDefaultValue(true);
        entity.Property(s => s.IsEditable).HasDefaultValue(true);
        entity.Property(s => s.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(s => !s.IsDeleted);

        // Indexes
        entity.HasIndex(s => s.Key).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_system_settings_key");
        entity.HasIndex(s => s.Group).HasDatabaseName("ix_system_settings_group");
        entity.HasIndex(s => s.IsVisible).HasDatabaseName("ix_system_settings_is_visible");
    }
}
