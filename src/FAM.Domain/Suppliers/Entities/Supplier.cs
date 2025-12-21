using FAM.Domain.Assets;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Geography;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Suppliers;

/// <summary>
/// Nhà cung cấp - Đầy đủ thông tin quản lý và đánh giá
/// </summary>
public class Supplier : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier, IHasDeletionTime,
    IHasDeleter
{
    // Basic Information
    public long? CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? ShortName { get; private set; }
    public string? Description { get; private set; }
    public string? SupplierCode { get; private set; } // Internal supplier code

    // Registration & Legal
    public string? TaxCode { get; private set; } // Tax ID / EIN
    public string? VATNumber { get; private set; } // VAT registration number
    public string? RegistrationNumber { get; private set; } // Business registration
    public string? DUNSNumber { get; private set; } // D&B number
    public string? GLN { get; private set; } // Global Location Number

    // Location
    public long? CountryId { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public PostalCode? PostalCode { get; private set; }
    public string? Region { get; private set; }

    // Contact Information
    public Url? Website { get; private set; }
    public Email? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public PhoneNumber? Fax { get; private set; }
    public PhoneNumber? MobilePhone { get; private set; }

    // Primary Contact Person
    public string? ContactPersonName { get; private set; }
    public string? ContactPersonTitle { get; private set; }
    public Email? ContactPersonEmail { get; private set; }
    public PhoneNumber? ContactPersonPhone { get; private set; }

    // Account Manager (their side)
    public string? AccountManagerName { get; private set; }
    public Email? AccountManagerEmail { get; private set; }
    public PhoneNumber? AccountManagerPhone { get; private set; }

    // Business Information
    public string? SupplierType { get; private set; } // Manufacturer, Distributor, Reseller, Service Provider
    public string? IndustryType { get; private set; }
    public string? BusinessType { get; private set; } // Corporation, LLC, Sole Proprietor, etc.
    public DateTime? EstablishedDate { get; private set; }
    public int? EmployeeCount { get; private set; }
    public Money? AnnualRevenue { get; private set; }
    public string? RevenueCurrency { get; private set; }

    // Financial Terms
    public string? PaymentTerms { get; private set; } // Net 30, Net 60, COD, etc.
    public string? PaymentMethods { get; private set; } // JSON array: Wire, Check, Credit Card
    public string? Currency { get; private set; } // Preferred currency
    public Money? CreditLimit { get; private set; }
    public Percentage? DiscountRate { get; private set; } // Standard discount %
    public bool TaxExempt { get; private set; }
    public string? TaxExemptCertificate { get; private set; }

    // Banking Information
    public string? BankName { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? BankRoutingNumber { get; private set; }
    public string? SwiftCode { get; private set; }
    public string? IBAN { get; private set; }

    // Certifications & Compliance
    public string? ISO9001Certified { get; private set; }
    public string? ISO14001Certified { get; private set; }
    public string? Certifications { get; private set; } // JSON array
    public bool IsMinorityOwned { get; private set; }
    public bool IsWomanOwned { get; private set; }
    public bool IsVeteranOwned { get; private set; }
    public bool IsSmallBusiness { get; private set; }

    // Product & Service Categories
    public string? ProductCategories { get; private set; } // JSON array
    public string? ServiceCategories { get; private set; } // JSON array
    public string? Specialization { get; private set; }

    // Performance & Rating
    public Rating? QualityRating { get; private set; }
    public Rating? DeliveryRating { get; private set; }
    public Rating? ServiceRating { get; private set; }
    public Rating? PriceRating { get; private set; }
    public int? OnTimeDeliveryPercentage { get; private set; }
    public int? DefectRate { get; private set; } // Percentage

    // Relationship & Status
    public string? SupplierStatus { get; private set; } // Active, Inactive, Blocked, Preferred
    public bool IsPreferred { get; private set; }
    public bool IsApproved { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public DateTime? ApprovedDate { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? PartnerSince { get; private set; }
    public DateTime? LastOrderDate { get; private set; }
    public DateTime? LastReviewDate { get; private set; }

    // Risk Assessment
    public string? RiskLevel { get; private set; } // Low, Medium, High
    public string? RiskFactors { get; private set; } // JSON array
    public bool RequiresInsurance { get; private set; }
    public bool RequiresBackgroundCheck { get; private set; }

    // Contract Information
    public string? ContractNumber { get; private set; }
    public DateTime? ContractStartDate { get; private set; }
    public DateTime? ContractEndDate { get; private set; }
    public Url? ContractDocumentUrl { get; private set; }
    public bool AutoRenew { get; private set; }

    // Shipping & Logistics
    public string? ShippingMethods { get; private set; } // JSON array
    public string? ShippingTerms { get; private set; } // FOB, CIF, DDP, etc.
    public int? LeadTimeDays { get; private set; }
    public Money? MinimumOrderValue { get; private set; }
    public string? MinimumOrderCurrency { get; private set; }
    public bool DropShipCapable { get; private set; }
    public string? WarehouseLocations { get; private set; } // JSON array

    // Support & Service
    public Email? SupportEmail { get; private set; }
    public PhoneNumber? SupportPhone { get; private set; }
    public string? SupportHours { get; private set; }
    public Url? SLADocumentUrl { get; private set; }
    public bool Provides24x7Support { get; private set; }

    // Insurance & Bonding
    public string? InsuranceProvider { get; private set; }
    public string? InsurancePolicyNumber { get; private set; }
    public Money? InsuranceCoverage { get; private set; }
    public DateTime? InsuranceExpiryDate { get; private set; }
    public string? BondingInformation { get; private set; }

    // Diversity & Sustainability
    public bool IsMBE { get; private set; } // Minority Business Enterprise
    public bool IsWBE { get; private set; } // Women Business Enterprise
    public bool IsSDVOSB { get; private set; } // Service-Disabled Veteran-Owned Small Business
    public bool IsEnvironmentallyCertified { get; private set; }
    public string? SustainabilityRating { get; private set; }

    // Internal Management
    public string? InternalNotes { get; private set; }
    public string? ProcurementNotes { get; private set; }
    public string? OurAccountManager { get; private set; } // Our internal account manager
    public string? Tags { get; private set; } // JSON array for categorization

    // Statistics
    public int? TotalOrders { get; private set; }
    public Money? TotalSpent { get; private set; }
    public Money? AverageOrderValue { get; private set; }

    // Documents & Attachments
    public Url? W9FormUrl { get; private set; } // US tax form
    public Url? CertificateOfInsuranceUrl { get; private set; }
    public Url? BusinessLicenseUrl { get; private set; }
    public string? References { get; private set; } // JSON array of references

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }

    public User? DeletedBy { get; set; }

    // public Companies.Company? Company { get; set; }
    public Country? Country { get; set; }

    private Supplier()
    {
    }

    public static Supplier Create(
        string name,
        string? taxCode = null,
        int? companyId = null,
        int? countryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required");

        return new Supplier
        {
            Name = name,
            TaxCode = taxCode,
            CompanyId = companyId,
            CountryId = countryId,
            IsActive = true,
            IsApproved = true,
            SupplierStatus = "Active"
        };
    }

    public void UpdateBasicInfo(
        string name,
        string? legalName,
        string? shortName,
        string? description,
        string? supplierCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required");

        Name = name;
        LegalName = legalName;
        ShortName = shortName;
        Description = description;
        SupplierCode = supplierCode;
    }

    public void UpdateRegistration(
        string? taxCode,
        string? vatNumber,
        string? registrationNumber,
        string? dunsNumber,
        string? gln)
    {
        TaxCode = taxCode;
        VATNumber = vatNumber;
        RegistrationNumber = registrationNumber;
        DUNSNumber = dunsNumber;
        GLN = gln;
    }

    public void UpdateLocation(
        int? countryId,
        string? address,
        string? city,
        string? state,
        string? postalCode,
        string? region)
    {
        CountryId = countryId;
        Address = address;
        City = city;
        State = state;
        PostalCode = postalCode != null ? PostalCode.Create(postalCode) : null;
        Region = region;
    }

    public void UpdateContactInfo(
        string? website,
        string? email,
        string? phone,
        string? fax,
        string? mobile)
    {
        Website = website != null ? Url.Create(website) : null;
        Email = email != null ? Email.Create(email) : null;
        Phone = phone != null ? PhoneNumber.Create(phone) : null;
        Fax = fax != null ? PhoneNumber.Create(fax) : null;
        MobilePhone = mobile != null ? PhoneNumber.Create(mobile) : null;
    }

    public void UpdatePrimaryContact(
        string? name,
        string? title,
        string? email,
        string? phone)
    {
        ContactPersonName = name;
        ContactPersonTitle = title;
        ContactPersonEmail = email != null ? Email.Create(email) : null;
        ContactPersonPhone = phone != null ? PhoneNumber.Create(phone) : null;
    }

    public void UpdateAccountManager(
        string? name,
        string? email,
        string? phone)
    {
        AccountManagerName = name;
        AccountManagerEmail = email != null ? Email.Create(email) : null;
        AccountManagerPhone = phone != null ? PhoneNumber.Create(phone) : null;
    }

    public void UpdateBusinessInfo(
        string? supplierType,
        string? industryType,
        string? businessType,
        DateTime? establishedDate,
        int? employeeCount)
    {
        if (employeeCount.HasValue && employeeCount.Value < 0)
            throw new DomainException("Employee count cannot be negative");

        SupplierType = supplierType;
        IndustryType = industryType;
        BusinessType = businessType;
        EstablishedDate = establishedDate;
        EmployeeCount = employeeCount;
    }

    public void UpdateFinancialTerms(
        string? paymentTerms,
        string? paymentMethods,
        string? currency,
        decimal? creditLimit,
        decimal? discountRate)
    {
        PaymentTerms = paymentTerms;
        PaymentMethods = paymentMethods;
        Currency = currency;
        CreditLimit = creditLimit.HasValue ? Money.Create(creditLimit.Value, currency ?? "VND") : null;
        DiscountRate = discountRate.HasValue ? Percentage.Create(discountRate.Value) : null;
    }

    public void UpdateBankingInfo(
        string? bankName,
        string? accountNumber,
        string? routingNumber,
        string? swiftCode,
        string? iban)
    {
        BankName = bankName;
        BankAccountNumber = accountNumber;
        BankRoutingNumber = routingNumber;
        SwiftCode = swiftCode;
        IBAN = iban;
    }

    public void UpdatePerformanceMetrics(
        int? onTimeDeliveryPercentage,
        int? defectRate)
    {
        if (onTimeDeliveryPercentage.HasValue &&
            (onTimeDeliveryPercentage.Value < 0 || onTimeDeliveryPercentage.Value > 100))
            throw new DomainException("On-time delivery percentage must be between 0 and 100");

        if (defectRate.HasValue && (defectRate.Value < 0 || defectRate.Value > 100))
            throw new DomainException("Defect rate must be between 0 and 100");

        OnTimeDeliveryPercentage = onTimeDeliveryPercentage;
        DefectRate = defectRate;
    }

    public void UpdateContract(
        string? contractNumber,
        DateTime? startDate,
        DateTime? endDate,
        string? documentUrl,
        bool autoRenew)
    {
        ContractNumber = contractNumber;
        ContractStartDate = startDate;
        ContractEndDate = endDate;
        ContractDocumentUrl = documentUrl != null ? Url.Create(documentUrl) : null;
        AutoRenew = autoRenew;
    }

    public void UpdateDocuments(
        string? w9FormUrl,
        string? certificateOfInsuranceUrl,
        string? businessLicenseUrl,
        string? references)
    {
        W9FormUrl = w9FormUrl != null ? Url.Create(w9FormUrl) : null;
        CertificateOfInsuranceUrl = certificateOfInsuranceUrl != null ? Url.Create(certificateOfInsuranceUrl) : null;
        BusinessLicenseUrl = businessLicenseUrl != null ? Url.Create(businessLicenseUrl) : null;
        References = references;
    }

    public void Approve(string approvedBy)
    {
        IsApproved = true;
        ApprovedDate = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        SupplierStatus = "Active";
    }

    public void Reject()
    {
        IsApproved = false;
        SupplierStatus = "Inactive";
    }

    public void Block()
    {
        SupplierStatus = "Blocked";
        IsActive = false;
    }

    public void SetAsPreferred()
    {
        IsPreferred = true;
        SupplierStatus = "Preferred";
    }

    public void RemovePreferred()
    {
        IsPreferred = false;
        if (SupplierStatus == "Preferred")
            SupplierStatus = "Active";
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public decimal GetAverageRating()
    {
        var ratings = new[] { QualityRating, DeliveryRating, ServiceRating, PriceRating }
            .Where(r => r != null)
            .Select(r => (decimal)r!.Value)
            .ToList();

        return ratings.Any() ? ratings.Average() : 0;
    }

    public bool IsContractExpiring(int daysThreshold = 30)
    {
        if (!ContractEndDate.HasValue)
            return false;

        return ContractEndDate.Value <= DateTime.UtcNow.AddDays(daysThreshold) &&
               ContractEndDate.Value > DateTime.UtcNow;
    }

    public bool IsContractExpired()
    {
        return ContractEndDate.HasValue && ContractEndDate.Value < DateTime.UtcNow;
    }

    public void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
