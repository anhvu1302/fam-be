using FAM.Domain.Manufacturers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> entity)
    {
        entity.HasKey(m => m.Id);
        entity.Property(m => m.Name).IsRequired().HasMaxLength(200);
        entity.Property(m => m.Description).HasMaxLength(1000);
        entity.Property(m => m.HeadquartersAddress).HasMaxLength(500);
        entity.Property(m => m.Email).HasMaxLength(255);
        entity.Property(m => m.Phone).HasMaxLength(50);
        entity.Property(m => m.TaxId).HasMaxLength(50);
        entity.Property(m => m.RegistrationNumber).HasMaxLength(50);
        entity.Property(m => m.IndustryType).HasMaxLength(100);
        entity.Property(m => m.Certifications).HasColumnType("text");
        entity.Property(m => m.WarrantyPolicy).HasColumnType("text");

        // Value object conversions for URLs
        entity.Property(m => m.LogoUrl).HasMaxLength(500);

        entity.Property(m => m.Website).HasMaxLength(500);

        entity.Property(m => m.SupportWebsite).HasMaxLength(500);

        entity.Property(m => m.LinkedInUrl).HasMaxLength(500);

        entity.Property(m => m.FacebookUrl).HasMaxLength(500);

        entity.Property(m => m.SLADocumentUrl).HasMaxLength(500);

        entity.Property(m => m.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(m => !m.IsDeleted);

        // Indexes
        entity.HasIndex(m => m.Name).HasDatabaseName("ix_manufacturers_name");
        entity.HasIndex(m => m.CountryId).HasDatabaseName("ix_manufacturers_country_id");
        entity.HasIndex(m => m.IsActive).HasDatabaseName("ix_manufacturers_is_active");

        // Relationships
        entity.HasOne(m => m.Country).WithMany().HasForeignKey(m => m.CountryId).OnDelete(DeleteBehavior.Restrict);
    }
}
