using FAM.Domain.Statuses;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class UsageStatusConfiguration : IEntityTypeConfiguration<UsageStatus>
{
    public void Configure(EntityTypeBuilder<UsageStatus> entity)
    {
        entity.HasKey(us => us.Id);
        entity.Property(us => us.Code).IsRequired().HasMaxLength(20);
        entity.Property(us => us.Name).IsRequired().HasMaxLength(100);
        entity.Property(us => us.Description).HasMaxLength(500);
        entity.Property(us => us.Color).HasMaxLength(20);
        entity.Property(us => us.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(us => !us.IsDeleted);
        entity.HasIndex(us => us.Code).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_usage_statuses_code");
    }
}
