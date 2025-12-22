using FAM.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(r => r.Id);

        // Value object conversion
        entity.Property(r => r.Code).IsRequired().HasMaxLength(50);

        entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
        entity.Property(r => r.Rank).IsRequired();
        entity.Property(r => r.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(r => !r.IsDeleted);
        entity.HasIndex(r => r.Code).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ix_roles_code");
        entity.HasIndex(r => r.Rank).HasDatabaseName("ix_roles_rank");
    }
}
