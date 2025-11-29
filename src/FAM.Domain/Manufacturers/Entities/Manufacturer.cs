using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Manufacturers;

/// <summary>
/// Nhà sản xuất - Đầy đủ thông tin theo chuẩn doanh nghiệp
/// </summary>
public class Manufacturer : Entity
{
    // Basic Information
    public string Name { get; private set; } = string.Empty;
    public string? ShortName { get; private set; } // Abbreviated name
    public string? LegalName { get; private set; } // Full legal name
    public string? Description { get; private set; }

    // Brand Information
    public string? BrandName { get; private set; }
    public Url? LogoUrl { get; private set; }
    public string? Tagline { get; private set; }

    // Company Details
    public string? RegistrationNumber { get; private set; } // Business registration number
    public string? TaxId { get; private set; } // Tax identification number
    public string? VATNumber { get; private set; } // VAT registration number
    public string? DUNSNumber { get; private set; } // Dun & Bradstreet number

    // Location
    public int? CountryId { get; private set; }
    public string? HeadquartersAddress { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }

    // Contact Information
    public Url? Website { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Fax { get; private set; }
    public string? SupportEmail { get; private set; }
    public string? SupportPhone { get; private set; }
    public Url? SupportWebsite { get; private set; }

    // Social Media
    public Url? LinkedInUrl { get; private set; }
    public string? TwitterHandle { get; private set; }
    public Url? FacebookUrl { get; private set; }

    // Business Information
    public string? IndustryType { get; private set; } // Electronics, Automotive, Software, etc.
    public string? BusinessType { get; private set; } // Public, Private, Government
    public DateTime? FoundedDate { get; private set; }
    public int? EmployeeCount { get; private set; }
    public decimal? AnnualRevenue { get; private set; }
    public string? RevenueCurrency { get; private set; }

    // Certification & Compliance
    public string? ISO9001Certified { get; private set; } // Quality Management
    public string? ISO14001Certified { get; private set; } // Environmental Management
    public string? ISO27001Certified { get; private set; } // Information Security
    public string? Certifications { get; private set; } // JSON array of other certifications

    // Warranty & Support
    public string? WarrantyPolicy { get; private set; }
    public int? StandardWarrantyMonths { get; private set; }
    public string? SupportHours { get; private set; } // Business hours or 24/7
    public Url? SLADocumentUrl { get; private set; }

    // Partner Information
    public bool IsPreferred { get; private set; } // Preferred manufacturer
    public bool IsApproved { get; private set; } = true; // Approved for procurement
    public bool IsActive { get; private set; } = true;
    public DateTime? PartnerSince { get; private set; }
    public string? AccountManager { get; private set; } // Our account manager name

    // Rating & Reviews
    public decimal? QualityRating { get; private set; } // 1-5 scale
    public decimal? ServiceRating { get; private set; } // 1-5 scale
    public decimal? PriceRating { get; private set; } // 1-5 scale
    public string? InternalNotes { get; private set; } // Private notes about manufacturer

    // Financial Terms
    public string? PaymentTerms { get; private set; } // Net 30, Net 60, etc.
    public string? PreferredCurrency { get; private set; }
    public decimal? DiscountRate { get; private set; } // Standard discount percentage

    // Stock & Availability
    public string? LeadTime { get; private set; } // Standard lead time
    public string? MinimumOrderQuantity { get; private set; }

    // Navigation properties
    public Geography.Country? Country { get; set; }
    public ICollection<Models.Model> Models { get; set; } = new List<Models.Model>();
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();

    private Manufacturer()
    {
    }

    public static Manufacturer Create(string name, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Manufacturer name is required");

        return new Manufacturer
        {
            Name = name,
            Website = website != null ? Url.Create(website) : null,
            IsActive = true,
            IsApproved = true
        };
    }

    public void UpdateBasicInfo(
        string name,
        string? shortName,
        string? legalName,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Manufacturer name is required");

        Name = name;
        ShortName = shortName;
        LegalName = legalName;
        Description = description;
    }

    public void UpdateBrand(
        string? brandName,
        string? logoUrl,
        string? tagline)
    {
        BrandName = brandName;
        LogoUrl = logoUrl != null ? Url.Create(logoUrl) : null;
        Tagline = tagline;
    }

    public void UpdateRegistration(
        string? registrationNumber,
        string? taxId,
        string? vatNumber,
        string? dunsNumber)
    {
        RegistrationNumber = registrationNumber;
        TaxId = taxId;
        VATNumber = vatNumber;
        DUNSNumber = dunsNumber;
    }

    public void UpdateLocation(
        int? countryId,
        string? address,
        string? city,
        string? state,
        string? postalCode)
    {
        CountryId = countryId;
        HeadquartersAddress = address;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public void UpdateContactInfo(
        string? website,
        string? email,
        string? phone,
        string? fax)
    {
        Website = website != null ? Url.Create(website) : null;
        Email = email;
        Phone = phone;
        Fax = fax;
    }

    public void UpdateSupport(
        string? supportEmail,
        string? supportPhone,
        string? supportWebsite,
        string? supportHours)
    {
        SupportEmail = supportEmail;
        SupportPhone = supportPhone;
        SupportWebsite = supportWebsite != null ? Url.Create(supportWebsite) : null;
        SupportHours = supportHours;
    }

    public void UpdateSocialMedia(
        string? linkedIn,
        string? twitter,
        string? facebook)
    {
        LinkedInUrl = linkedIn != null ? Url.Create(linkedIn) : null;
        TwitterHandle = twitter;
        FacebookUrl = facebook != null ? Url.Create(facebook) : null;
    }

    public void UpdateBusinessInfo(
        string? industryType,
        string? businessType,
        DateTime? foundedDate,
        int? employeeCount)
    {
        if (employeeCount.HasValue && employeeCount.Value < 0)
            throw new DomainException("Employee count cannot be negative");

        IndustryType = industryType;
        BusinessType = businessType;
        FoundedDate = foundedDate;
        EmployeeCount = employeeCount;
    }

    public void UpdateFinancialInfo(
        decimal? annualRevenue,
        string? revenueCurrency,
        string? paymentTerms,
        string? preferredCurrency,
        decimal? discountRate)
    {
        if (annualRevenue.HasValue && annualRevenue.Value < 0)
            throw new DomainException("Annual revenue cannot be negative");

        if (discountRate.HasValue && (discountRate.Value < 0 || discountRate.Value > 100))
            throw new DomainException("Discount rate must be between 0 and 100");

        AnnualRevenue = annualRevenue;
        RevenueCurrency = revenueCurrency;
        PaymentTerms = paymentTerms;
        PreferredCurrency = preferredCurrency;
        DiscountRate = discountRate;
    }

    public void UpdateCertifications(
        string? iso9001,
        string? iso14001,
        string? iso27001,
        string? certifications)
    {
        ISO9001Certified = iso9001;
        ISO14001Certified = iso14001;
        ISO27001Certified = iso27001;
        Certifications = certifications;
    }

    public void UpdateWarrantySupport(
        string? warrantyPolicy,
        int? standardWarrantyMonths,
        string? slaDocumentUrl)
    {
        if (standardWarrantyMonths.HasValue && standardWarrantyMonths.Value < 0)
            throw new DomainException("Standard warranty months cannot be negative");

        WarrantyPolicy = warrantyPolicy;
        StandardWarrantyMonths = standardWarrantyMonths;
        SLADocumentUrl = slaDocumentUrl != null ? Url.Create(slaDocumentUrl) : null;
    }

    public void UpdatePartnerInfo(
        bool isPreferred,
        bool isApproved,
        DateTime? partnerSince,
        string? accountManager)
    {
        IsPreferred = isPreferred;
        IsApproved = isApproved;
        PartnerSince = partnerSince;
        AccountManager = accountManager;
    }

    public void UpdateRatings(
        decimal? qualityRating,
        decimal? serviceRating,
        decimal? priceRating,
        string? internalNotes)
    {
        if (qualityRating.HasValue && (qualityRating < 1 || qualityRating > 5))
            throw new DomainException("Rating must be between 1 and 5");
        if (serviceRating.HasValue && (serviceRating < 1 || serviceRating > 5))
            throw new DomainException("Rating must be between 1 and 5");
        if (priceRating.HasValue && (priceRating < 1 || priceRating > 5))
            throw new DomainException("Rating must be between 1 and 5");

        QualityRating = qualityRating;
        ServiceRating = serviceRating;
        PriceRating = priceRating;
        InternalNotes = internalNotes;
    }

    public void UpdateStockInfo(
        string? leadTime,
        string? minimumOrderQuantity)
    {
        LeadTime = leadTime;
        MinimumOrderQuantity = minimumOrderQuantity;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Approve()
    {
        IsApproved = true;
    }

    public void Reject()
    {
        IsApproved = false;
    }

    public void SetAsPreferred()
    {
        IsPreferred = true;
    }

    public void RemovePreferred()
    {
        IsPreferred = false;
    }

    public decimal GetAverageRating()
    {
        var ratings = new[] { QualityRating, ServiceRating, PriceRating }
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .ToList();

        return ratings.Any() ? ratings.Average() : 0;
    }
}