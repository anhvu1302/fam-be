using FAM.Domain.Statuses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class LifecycleStatusConfiguration : IEntityTypeConfiguration<LifecycleStatus>
{
    public void Configure(EntityTypeBuilder<LifecycleStatus> entity)
    {
        entity.HasKey(ls => ls.Id);
        entity.Property(ls => ls.Code).IsRequired().HasMaxLength(20);
        entity.Property(ls => ls.Name).IsRequired().HasMaxLength(100);
        entity.Property(ls => ls.Description).HasMaxLength(500);
        entity.Property(ls => ls.Color).HasMaxLength(20);
        entity.Property(ls => ls.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(ls => !ls.IsDeleted);
        entity.HasIndex(ls => ls.Code).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_lifecycle_statuses_code");
    }
}
