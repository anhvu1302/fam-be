using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Supplier
/// </summary>
[Table("suppliers")]
public class SupplierEf : BaseEntityEf
{
    // Basic Information
    public long? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string? SupplierCode { get; set; }
    public string? Code { get; set; }
    public string? LogoUrl { get; set; }

    // Registration & Legal
    public string? TaxCode { get; set; }
    public string? TaxId { get; set; }
    public string? VATNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? DUNSNumber { get; set; }
    public string? GLN { get; set; }

    // Location
    public long? CountryId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Region { get; set; }

    // Contact Information
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? MobilePhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    // Primary Contact Person
    public string? ContactPersonName { get; set; }
    public string? ContactPersonTitle { get; set; }
    public string? ContactPersonEmail { get; set; }
    public string? ContactPersonPhone { get; set; }

    // Account Manager
    public string? AccountManagerName { get; set; }
    public string? AccountManagerEmail { get; set; }
    public string? AccountManagerPhone { get; set; }

    // Business Information
    public string? SupplierType { get; set; }
    public string? IndustryType { get; set; }
    public string? Industry { get; set; }
    public string? BusinessType { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? RevenueCurrency { get; set; }

    // Financial Terms
    public string? PaymentTerms { get; set; }
    public string? PaymentMethods { get; set; }
    public string? Currency { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? DiscountRate { get; set; }
    public bool TaxExempt { get; set; }
    public string? TaxExemptCertificate { get; set; }

    // Banking Information
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string? SwiftCode { get; set; }
    public string? IBAN { get; set; }

    // Certifications & Compliance
    public string? ISO9001Certified { get; set; }
    public string? ISO14001Certified { get; set; }
    public string? Certifications { get; set; }
    public bool IsMinorityOwned { get; set; }
    public bool IsWomanOwned { get; set; }
    public bool IsVeteranOwned { get; set; }
    public bool IsSmallBusiness { get; set; }

    // Product & Service Categories
    public string? ProductCategories { get; set; }
    public string? ServiceCategories { get; set; }
    public string? Specialization { get; set; }

    // Performance & Rating
    public int? QualityRating { get; set; }
    public int? DeliveryRating { get; set; }
    public int? ServiceRating { get; set; }
    public int? PriceRating { get; set; }
    public int? OnTimeDeliveryPercentage { get; set; }
    public int? DefectRate { get; set; }

    // Relationship & Status
    public string? SupplierStatus { get; set; }
    public bool IsPreferred { get; set; }
    public bool IsApproved { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? PartnerSince { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public DateTime? LastReviewDate { get; set; }

    // Risk Assessment
    public string? RiskLevel { get; set; }
    public string? RiskFactors { get; set; }
    public bool RequiresInsurance { get; set; }
    public bool RequiresBackgroundCheck { get; set; }

    // Contract Information
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? ContractDocumentUrl { get; set; }
    public bool AutoRenew { get; set; }

    // Shipping & Logistics
    public string? ShippingMethods { get; set; }
    public string? ShippingTerms { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderValue { get; set; }
    public string? MinimumOrderCurrency { get; set; }
    public bool DropShipCapable { get; set; }
    public string? WarehouseLocations { get; set; }

    // Support & Service
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportHours { get; set; }
    public string? SLADocumentUrl { get; set; }
    public bool Provides24x7Support { get; set; }

    // Insurance & Bonding
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal? InsuranceCoverage { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public string? BondingInformation { get; set; }

    // Diversity & Sustainability
    public bool IsMBE { get; set; }
    public bool IsWBE { get; set; }
    public bool IsSDVOSB { get; set; }
    public bool IsEnvironmentallyCertified { get; set; }
    public string? SustainabilityRating { get; set; }

    // Internal Management
    public string? InternalNotes { get; set; }
    public string? Notes { get; set; }
    public string? ProcurementNotes { get; set; }
    public string? OurAccountManager { get; set; }
    public string? Tags { get; set; }

    // Statistics
    public int? TotalOrders { get; set; }
    public decimal? TotalSpent { get; set; }
    public string? TotalSpentCurrency { get; set; }
    public decimal? AverageOrderValue { get; set; }

    // Documents & Attachments
    public string? W9FormUrl { get; set; }
    public string? CertificateOfInsuranceUrl { get; set; }
    public string? BusinessLicenseUrl { get; set; }

    public string? References { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }


    // Navigation properties
    public CompanyDetailsEf? Company { get; set; }
    public CountryEf? Country { get; set; }
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}
