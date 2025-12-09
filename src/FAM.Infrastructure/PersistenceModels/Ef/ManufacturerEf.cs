using System.ComponentModel.DataAnnotations.Schema;
using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Manufacturer
/// </summary>
[Table("manufacturers")]
public class ManufacturerEf : BaseEntityEf
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ShortName { get; set; }
    public string? LegalName { get; set; }
    public string? Description { get; set; }

    // Brand Information
    public string? BrandName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Tagline { get; set; }

    // Company Details
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? VATNumber { get; set; }
    public string? DUNSNumber { get; set; }

    // Location
    public long? CountryId { get; set; }
    public string? HeadquartersAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }

    // Contact Information
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? SupportContact { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportWebsite { get; set; }

    // Social Media
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }

    // Business Information
    public string? Industry { get; set; }
    public string? IndustryType { get; set; }
    public string? BusinessType { get; set; }
    public DateTime? FoundedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? RevenueCurrency { get; set; }

    // Certification & Compliance
    public string? ISO9001Certified { get; set; }
    public string? ISO14001Certified { get; set; }
    public string? ISO27001Certified { get; set; }
    public string? Certifications { get; set; }

    // Warranty & Support
    public string? WarrantyTerms { get; set; }
    public string? WarrantyPolicy { get; set; }
    public int? StandardWarrantyMonths { get; set; }
    public string? SupportHours { get; set; }
    public string? SLADocumentUrl { get; set; }

    // Partner Information
    public bool IsPreferred { get; set; }
    public bool IsApproved { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime? PartnerSince { get; set; }
    public string? AccountManager { get; set; }

    // Rating & Reviews
    public decimal? QualityRating { get; set; }
    public decimal? ServiceRating { get; set; }
    public decimal? PriceRating { get; set; }
    public string? InternalNotes { get; set; }
    public string? Notes { get; set; }

    // Financial Terms
    public string? PaymentTerms { get; set; }
    public string? PreferredCurrency { get; set; }
    public decimal? DiscountRate { get; set; }

    // Stock & Availability
    public string? LeadTime { get; set; }

    public string? MinimumOrderQuantity { get; set; }

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
    public CountryEf? Country { get; set; }
    public ICollection<ModelEf> Models { get; set; } = new List<ModelEf>();
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}