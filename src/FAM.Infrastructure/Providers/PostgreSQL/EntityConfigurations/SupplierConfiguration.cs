using FAM.Domain.Suppliers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> entity)
    {
        entity.HasKey(s => s.Id);
        entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
        entity.Property(s => s.SupplierCode).HasMaxLength(50);
        entity.Property(s => s.Description).HasMaxLength(1000);
        entity.Property(s => s.Address).HasMaxLength(500);
        entity.Property(s => s.TaxCode).HasMaxLength(50);
        entity.Property(s => s.VATNumber).HasMaxLength(50);
        entity.Property(s => s.RegistrationNumber).HasMaxLength(50);
        entity.Property(s => s.IndustryType).HasMaxLength(100);
        entity.Property(s => s.PaymentTerms).HasMaxLength(200);
        entity.Property(s => s.ContractStartDate).HasColumnType("date");
        entity.Property(s => s.ContractEndDate).HasColumnType("date");

        // Primitive properties (no longer Value Objects)
        entity.Property(s => s.PostalCode)
            .HasMaxLength(20);

        entity.Property(s => s.Website)
            .HasMaxLength(500);

        entity.Property(s => s.Email)
            .HasMaxLength(255);

        entity.Property(s => s.Phone)
            .HasMaxLength(20);

        entity.Property(s => s.Fax)
            .HasMaxLength(20);

        entity.Property(s => s.MobilePhone)
            .HasMaxLength(20);

        entity.Property(s => s.ContactPersonEmail)
            .HasMaxLength(255);

        entity.Property(s => s.ContactPersonPhone)
            .HasMaxLength(20);

        entity.Property(s => s.AccountManagerEmail)
            .HasMaxLength(255);

        entity.Property(s => s.AccountManagerPhone)
            .HasMaxLength(20);

        // Money value objects - stored as complex owned entities
        entity.OwnsOne(s => s.AnnualRevenue, money =>
        {
            money.Property(m => m.Amount).HasColumnName("annual_revenue_amount");
            money.Property(m => m.Currency).HasColumnName("annual_revenue_currency").HasMaxLength(3);
        });

        entity.OwnsOne(s => s.CreditLimit, money =>
        {
            money.Property(m => m.Amount).HasColumnName("credit_limit_amount");
            money.Property(m => m.Currency).HasColumnName("credit_limit_currency").HasMaxLength(3);
        });

        entity.Property(s => s.DiscountRate);

        entity.Property(s => s.ContractDocumentUrl)
            .HasMaxLength(500);

        entity.OwnsOne(s => s.MinimumOrderValue, money =>
        {
            money.Property(m => m.Amount).HasColumnName("minimum_order_value_amount");
            money.Property(m => m.Currency).HasColumnName("minimum_order_value_currency").HasMaxLength(3);
        });

        entity.Property(s => s.SupportEmail)
            .HasMaxLength(255);

        entity.Property(s => s.SupportPhone)
            .HasMaxLength(20);

        entity.Property(s => s.SLADocumentUrl)
            .HasMaxLength(500);

        entity.OwnsOne(s => s.InsuranceCoverage, money =>
        {
            money.Property(m => m.Amount).HasColumnName("insurance_coverage_amount");
            money.Property(m => m.Currency).HasColumnName("insurance_coverage_currency").HasMaxLength(3);
        });

        entity.OwnsOne(s => s.TotalSpent, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_spent_amount");
            money.Property(m => m.Currency).HasColumnName("total_spent_currency").HasMaxLength(3);
        });

        entity.OwnsOne(s => s.AverageOrderValue, money =>
        {
            money.Property(m => m.Amount).HasColumnName("average_order_value_amount");
            money.Property(m => m.Currency).HasColumnName("average_order_value_currency").HasMaxLength(3);
        });

        entity.Property(s => s.W9FormUrl)
            .HasMaxLength(500);

        entity.Property(s => s.CertificateOfInsuranceUrl)
            .HasMaxLength(500);

        entity.Property(s => s.BusinessLicenseUrl)
            .HasMaxLength(500);
        entity.Property(s => s.IsDeleted).HasDefaultValue(false);

        entity.HasQueryFilter(s => !s.IsDeleted);

        // Indexes
        entity.HasIndex(s => s.SupplierCode).IsUnique()
            .HasFilter("is_deleted = false AND supplier_code IS NOT NULL")
            .HasDatabaseName("ix_suppliers_supplier_code");
        entity.HasIndex(s => s.Name).HasDatabaseName("ix_suppliers_name");
        entity.HasIndex(s => s.CountryId).HasDatabaseName("ix_suppliers_country_id");
        entity.HasIndex(s => s.IsActive).HasDatabaseName("ix_suppliers_is_active");

        // Relationships
        entity.HasOne(s => s.Country).WithMany().HasForeignKey(s => s.CountryId).OnDelete(DeleteBehavior.Restrict);
    }
}
