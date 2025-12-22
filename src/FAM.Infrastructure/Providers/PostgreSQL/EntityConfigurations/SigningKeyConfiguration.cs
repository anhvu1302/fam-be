using FAM.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> entity)
    {
        entity.HasKey(sk => sk.Id);
        entity.Property(sk => sk.KeyId).IsRequired().HasMaxLength(100);
        entity.Property(sk => sk.PublicKey).IsRequired();
        entity.Property(sk => sk.PrivateKey).IsRequired();
        entity.Property(sk => sk.Algorithm).IsRequired().HasMaxLength(20);
        entity.Property(sk => sk.KeySize).IsRequired();
        entity.Property(sk => sk.Use).IsRequired().HasMaxLength(10);
        entity.Property(sk => sk.KeyType).IsRequired().HasMaxLength(10);
        entity.Property(sk => sk.IsActive).HasDefaultValue(true);
        entity.Property(sk => sk.IsRevoked).HasDefaultValue(false);
        entity.Property(sk => sk.RevocationReason).HasMaxLength(500);
        entity.Property(sk => sk.Description).HasMaxLength(500);
        entity.Property(sk => sk.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(sk => !sk.IsDeleted);

        // Indexes
        entity.HasIndex(sk => sk.KeyId).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_signing_keys_key_id");
        entity.HasIndex(sk => sk.IsActive).HasDatabaseName("ix_signing_keys_is_active");
        entity.HasIndex(sk => sk.IsRevoked).HasDatabaseName("ix_signing_keys_is_revoked");
        entity.HasIndex(sk => sk.ExpiresAt).HasDatabaseName("ix_signing_keys_expires_at");
    }
}
