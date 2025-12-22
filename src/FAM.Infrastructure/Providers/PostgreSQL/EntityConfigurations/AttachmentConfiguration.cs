using FAM.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> entity)
    {
        entity.HasKey(att => att.Id);
        entity.Property(att => att.FileName).HasMaxLength(255);
        entity.Property(att => att.FileUrl).HasMaxLength(1000);
        entity.Property(att => att.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(att => !att.IsDeleted);

        // Indexes
        entity.HasIndex(att => att.AssetId).HasDatabaseName("ix_attachments_asset_id");
        entity.HasIndex(att => att.UploadedBy).HasDatabaseName("ix_attachments_uploaded_by");
        entity.HasIndex(att => att.UploadedAt).HasDatabaseName("ix_attachments_uploaded_at");

        // Relationships
        entity.HasOne(att => att.Asset).WithMany(a => a.Attachments).HasForeignKey(att => att.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(att => att.Uploader).WithMany().HasForeignKey(att => att.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
