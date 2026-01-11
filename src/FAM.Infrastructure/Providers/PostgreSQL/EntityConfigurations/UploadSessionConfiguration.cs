using FAM.Domain.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class UploadSessionConfiguration : IEntityTypeConfiguration<UploadSession>
{
    public void Configure(EntityTypeBuilder<UploadSession> entity)
    {
        entity.HasKey(us => us.Id);

        entity.Property(us => us.UploadId).IsRequired().HasMaxLength(100);
        entity.Property(us => us.TempKey).IsRequired().HasMaxLength(500);
        entity.Property(us => us.FileName).IsRequired().HasMaxLength(500);
        entity.Property(us => us.FileType).IsRequired();
        entity.Property(us => us.FileSize).IsRequired();
        entity.Property(us => us.ContentType).IsRequired().HasMaxLength(200);
        entity.Property(us => us.Status).IsRequired();
        entity.Property(us => us.ExpiresAt).IsRequired();
        entity.Property(us => us.FinalKey).HasMaxLength(500);
        entity.Property(us => us.EntityId);
        entity.Property(us => us.EntityType).HasMaxLength(100);
        entity.Property(us => us.Checksum).HasMaxLength(128);
        entity.Property(us => us.UserId).IsRequired();
        entity.Property(us => us.IdempotencyKey).HasMaxLength(100);

        // Audit fields
        entity.Property(us => us.CreatedAt).IsRequired();
        entity.Property(us => us.CreatedById);
        entity.Property(us => us.UpdatedAt);
        entity.Property(us => us.IsDeleted).HasDefaultValue(false);
        entity.Property(us => us.DeletedAt);

        // Query filter for soft delete
        entity.HasQueryFilter(us => !us.IsDeleted);

        // Indexes
        entity.HasIndex(us => us.UploadId).IsUnique().HasDatabaseName("ix_upload_sessions_upload_id");
        entity.HasIndex(us => us.UserId).HasDatabaseName("ix_upload_sessions_user_id");
        entity.HasIndex(us => us.Status).HasDatabaseName("ix_upload_sessions_status");
        entity.HasIndex(us => us.CreatedAt).HasDatabaseName("ix_upload_sessions_created_at");
        entity.HasIndex(us => us.ExpiresAt).HasDatabaseName("ix_upload_sessions_expires_at");
        entity.HasIndex(us => us.IdempotencyKey)
            .HasDatabaseName("ix_upload_sessions_idempotency_key")
            .HasFilter("idempotency_key IS NOT NULL");
        entity.HasIndex(us => new { us.EntityId, us.EntityType })
            .HasDatabaseName("ix_upload_sessions_entity")
            .HasFilter("entity_id IS NOT NULL");
    }
}
