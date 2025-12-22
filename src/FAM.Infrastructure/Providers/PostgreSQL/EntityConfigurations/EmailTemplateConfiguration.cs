using FAM.Domain.EmailTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
        entity.Property(e => e.HtmlBody).IsRequired();
        entity.Property(e => e.PlainTextBody);
        entity.Property(e => e.Description).HasMaxLength(1000);
        entity.Property(e => e.AvailablePlaceholders);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsSystem).HasDefaultValue(false);
        entity.Property(e => e.Category).IsRequired();
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(e => !e.IsDeleted);

        // Indexes
        entity.HasIndex(e => e.Code).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_email_templates_code");
        entity.HasIndex(e => e.Category).HasDatabaseName("ix_email_templates_category");
        entity.HasIndex(e => e.IsActive).HasDatabaseName("ix_email_templates_is_active");
        entity.HasIndex(e => e.IsSystem).HasDatabaseName("ix_email_templates_is_system");
    }
}
