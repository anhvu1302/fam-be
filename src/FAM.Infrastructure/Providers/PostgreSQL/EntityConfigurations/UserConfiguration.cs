using FAM.Domain.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);

        // Primitive properties (no longer Value Objects)
        entity.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        // Password remains as OwnsOne (complex Value Object)
        entity.OwnsOne(u => u.Password, password =>
        {
            password.Property(p => p.Hash).HasColumnName("password_hash").IsRequired();
            password.Property(p => p.Salt).HasColumnName("password_salt").IsRequired();
        });

        entity.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        entity.Property(u => u.PhoneCountryCode)
            .HasMaxLength(10);

        entity.Property(u => u.FullName).HasMaxLength(200);
        entity.Property(u => u.IsDeleted).HasDefaultValue(false);

        // Pending 2FA fields configuration
        entity.Property(u => u.PendingTwoFactorSecret).HasMaxLength(128);
        entity.Property(u => u.PendingTwoFactorSecretExpiresAt);

        // Soft delete filter
        entity.HasQueryFilter(u => !u.IsDeleted);

        // Indexes - explicitly name them to ensure snake_case
        entity.HasIndex(u => u.Username).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_users_username");
        entity.HasIndex(u => u.Email).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_users_email");
    }
}
