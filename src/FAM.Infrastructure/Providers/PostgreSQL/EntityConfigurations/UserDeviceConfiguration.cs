using FAM.Domain.Users.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> entity)
    {
        entity.HasKey(ud => ud.Id);

        // Configure UUID generation
        entity.Property(ud => ud.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        entity.Property(ud => ud.UserId).IsRequired();
        entity.Property(ud => ud.DeviceId).IsRequired().HasMaxLength(255);
        entity.Property(ud => ud.DeviceName).IsRequired().HasMaxLength(200);
        entity.Property(ud => ud.DeviceType).IsRequired().HasMaxLength(50);
        entity.Property(ud => ud.UserAgent).HasMaxLength(500);
        entity.Property(ud => ud.Location).HasMaxLength(255);
        entity.Property(ud => ud.Browser).HasMaxLength(100);
        entity.Property(ud => ud.OperatingSystem).HasMaxLength(100);

        // Value object as owned entity for IPAddress
        entity.OwnsOne(ud => ud.IpAddress, ip =>
        {
            ip.Property(i => i.Value).HasColumnName("ip_address").HasMaxLength(45);
            ip.Property(i => i.Type).HasColumnName("ip_address_type").HasMaxLength(10);
        });

        entity.Property(ud => ud.RefreshToken).HasMaxLength(500);
        entity.Property(ud => ud.ActiveAccessTokenJti).HasMaxLength(255);
        entity.Property(ud => ud.IsActive).HasDefaultValue(true);
        entity.Property(ud => ud.IsTrusted).HasDefaultValue(false);
        entity.Property(ud => ud.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(ud => !ud.IsDeleted);

        // Indexes - DeviceId should be unique per user
        entity.HasIndex(ud => new { ud.DeviceId, ud.UserId })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_user_devices_device_user");
        entity.HasIndex(ud => ud.UserId).HasDatabaseName("ix_user_devices_user_id");
        entity.HasIndex(ud => new { ud.UserId, ud.IsActive }).HasDatabaseName("ix_user_devices_user_active");
        entity.HasIndex(ud => ud.LastLoginAt).HasDatabaseName("ix_user_devices_last_login");
        entity.HasIndex(ud => ud.CreatedAt).HasDatabaseName("ix_user_devices_created_at");

        // Relationships
        entity.HasOne(ud => ud.User)
            .WithMany(u => u.UserDevices)
            .HasForeignKey(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
