using FAM.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> entity)
    {
        entity.HasKey(l => l.Id);
        entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
        entity.Property(l => l.Code).HasMaxLength(50);
        entity.Property(l => l.Type).HasMaxLength(50);
        entity.Property(l => l.FullPath).HasMaxLength(1000);
        entity.Property(l => l.PathIds).HasMaxLength(1000);
        entity.Property(l => l.Description).HasMaxLength(500);
        entity.Property(l => l.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(l => !l.IsDeleted);

        // Indexes
        entity.HasIndex(l => l.Code).IsUnique().HasFilter("is_deleted = false AND code IS NOT NULL")
            .HasDatabaseName("ix_locations_code");
        entity.HasIndex(l => l.Name).HasDatabaseName("ix_locations_name");
        entity.HasIndex(l => l.ParentId).HasDatabaseName("ix_locations_parent_id");
        entity.HasIndex(l => l.CompanyId).HasDatabaseName("ix_locations_company_id");
        entity.HasIndex(l => l.CountryId).HasDatabaseName("ix_locations_country_id");

        // Relationships
        entity.HasOne(l => l.Company).WithMany().HasForeignKey(l => l.CompanyId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(l => l.Country).WithMany().HasForeignKey(l => l.CountryId).OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship
        entity.HasOne(l => l.Parent)
            .WithMany(l => l.Children)
            .HasForeignKey(l => l.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
