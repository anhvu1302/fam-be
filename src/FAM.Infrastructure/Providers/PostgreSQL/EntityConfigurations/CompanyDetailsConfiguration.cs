using FAM.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class CompanyDetailsConfiguration : IEntityTypeConfiguration<CompanyDetails>
{
    public void Configure(EntityTypeBuilder<CompanyDetails> entity)
    {
        entity.HasKey(cd => cd.Id);
        entity.Property(cd => cd.NodeId).IsRequired();

        // Value object conversions
        entity.Property(cd => cd.TaxCode).HasMaxLength(50);

        entity.Property(cd => cd.Domain).HasMaxLength(255);

        // Owned entity for Address - mapped to same table
        entity.OwnsOne(cd => cd.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(255);
            address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100);
            address.Property(a => a.Ward).HasColumnName("address_ward").HasMaxLength(100);
            address.Property(a => a.District).HasColumnName("address_district").HasMaxLength(100);
            address.Property(a => a.Province).HasColumnName("address_province").HasMaxLength(100);
            address.Property(a => a.CountryCode).HasColumnName("address_country_code").HasMaxLength(10);
            address.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(20);
        });

        entity.Property(cd => cd.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(cd => !cd.IsDeleted);
        entity.HasIndex(cd => cd.NodeId).IsUnique().HasFilter("is_deleted = false")
            .HasDatabaseName("ix_company_details_node_id");
        entity.HasIndex(cd => cd.TaxCode).IsUnique().HasFilter("is_deleted = false AND tax_code IS NOT NULL")
            .HasDatabaseName("ix_company_details_tax_code");
        entity.HasIndex(cd => cd.Domain).IsUnique().HasFilter("is_deleted = false AND domain IS NOT NULL")
            .HasDatabaseName("ix_company_details_domain");

        // Relationship
        entity.HasOne(cd => cd.Node)
            .WithOne(n => n.CompanyDetails)
            .HasForeignKey<CompanyDetails>(cd => cd.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
