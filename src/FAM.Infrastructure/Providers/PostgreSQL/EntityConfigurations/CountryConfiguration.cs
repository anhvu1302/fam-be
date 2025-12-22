using FAM.Domain.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> entity)
    {
        entity.HasKey(c => c.Id);

        // Value object conversion
        entity.Property(c => c.Code).IsRequired().HasMaxLength(2);

        entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        entity.Property(c => c.Iso3Code).HasMaxLength(3);
        entity.Property(c => c.NumericCode).HasMaxLength(3);
        entity.Property(c => c.Capital).HasMaxLength(100);
        entity.Property(c => c.Region).HasMaxLength(50);
        entity.Property(c => c.SubRegion).HasMaxLength(50);
        entity.Property(c => c.CurrencyCode).HasMaxLength(3);
        entity.Property(c => c.CurrencyName).HasMaxLength(50);
        entity.Property(c => c.PhoneCode).HasMaxLength(10);
        entity.Property(c => c.TLD).HasMaxLength(5);
        entity.Property(c => c.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(c => !c.IsDeleted);

        // Indexes
        entity.HasIndex(c => c.Iso3Code).IsUnique().HasFilter("is_deleted = false AND iso3_code IS NOT NULL")
            .HasDatabaseName("ix_countries_iso3_code");
        entity.HasIndex(c => c.Name).HasDatabaseName("ix_countries_name");
    }
}
