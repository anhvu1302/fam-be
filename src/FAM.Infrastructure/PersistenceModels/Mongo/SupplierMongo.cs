using FAM.Infrastructure.PersistenceModels.Mongo.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for Supplier
/// </summary>
[BsonIgnoreExtraElements]
public class SupplierMongo : BaseEntityMongo
{
    // Basic Information
    [BsonElement("companyId")] public long? CompanyId { get; set; }

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("legalName")] public string? LegalName { get; set; }

    [BsonElement("shortName")] public string? ShortName { get; set; }

    [BsonElement("description")] public string? Description { get; set; }

    [BsonElement("supplierCode")] public string? SupplierCode { get; set; }

    [BsonElement("code")] public string? Code { get; set; }

    // Registration & Legal
    [BsonElement("taxCode")] public string? TaxCode { get; set; }

    [BsonElement("taxId")] public string? TaxId { get; set; }

    [BsonElement("vatNumber")] public string? VATNumber { get; set; }

    [BsonElement("registrationNumber")] public string? RegistrationNumber { get; set; }

    [BsonElement("dunsNumber")] public string? DUNSNumber { get; set; }

    [BsonElement("gln")] public string? GLN { get; set; }

    // Location
    [BsonElement("countryId")] public long? CountryId { get; set; }

    [BsonElement("address")] public string? Address { get; set; }

    [BsonElement("city")] public string? City { get; set; }

    [BsonElement("state")] public string? State { get; set; }

    [BsonElement("postalCode")] public string? PostalCode { get; set; }

    [BsonElement("region")] public string? Region { get; set; }

    // Contact Information
    [BsonElement("website")] public string? Website { get; set; }

    [BsonElement("email")] public string? Email { get; set; }

    [BsonElement("phone")] public string? Phone { get; set; }

    [BsonElement("fax")] public string? Fax { get; set; }

    [BsonElement("mobilePhone")] public string? MobilePhone { get; set; }

    [BsonElement("logoUrl")] public string? LogoUrl { get; set; }

    [BsonElement("contactEmail")] public string? ContactEmail { get; set; }

    [BsonElement("contactPhone")] public string? ContactPhone { get; set; }

    // Primary Contact Person
    [BsonElement("contactPersonName")] public string? ContactPersonName { get; set; }

    [BsonElement("contactPersonTitle")] public string? ContactPersonTitle { get; set; }

    // Account Manager
    [BsonElement("accountManagerName")] public string? AccountManagerName { get; set; }

    [BsonElement("accountManagerEmail")] public string? AccountManagerEmail { get; set; }

    [BsonElement("accountManagerPhone")] public string? AccountManagerPhone { get; set; }

    // Business Information
    [BsonElement("supplierType")] public string? SupplierType { get; set; }

    [BsonElement("industryType")] public string? IndustryType { get; set; }

    [BsonElement("industry")] public string? Industry { get; set; }

    [BsonElement("businessType")] public string? BusinessType { get; set; }

    [BsonElement("establishedDate")] public DateTime? EstablishedDate { get; set; }

    [BsonElement("employeeCount")] public int? EmployeeCount { get; set; }

    [BsonElement("annualRevenue")] public decimal? AnnualRevenue { get; set; }

    [BsonElement("revenueCurrency")] public string? RevenueCurrency { get; set; }

    // Financial Terms
    [BsonElement("paymentTerms")] public string? PaymentTerms { get; set; }

    [BsonElement("paymentMethods")] public string? PaymentMethods { get; set; }

    [BsonElement("currency")] public string? Currency { get; set; }

    [BsonElement("creditLimit")] public decimal? CreditLimit { get; set; }

    [BsonElement("discountRate")] public decimal? DiscountRate { get; set; }

    [BsonElement("taxExempt")] public bool TaxExempt { get; set; }

    [BsonElement("taxExemptCertificate")] public string? TaxExemptCertificate { get; set; }

    // Banking Information
    [BsonElement("bankName")] public string? BankName { get; set; }

    [BsonElement("bankAccountNumber")] public string? BankAccountNumber { get; set; }

    [BsonElement("bankRoutingNumber")] public string? BankRoutingNumber { get; set; }

    [BsonElement("swiftCode")] public string? SwiftCode { get; set; }

    [BsonElement("iban")] public string? IBAN { get; set; }

    // Certifications & Compliance
    [BsonElement("iso9001Certified")] public string? ISO9001Certified { get; set; }

    [BsonElement("iso14001Certified")] public string? ISO14001Certified { get; set; }

    [BsonElement("certifications")] public string? Certifications { get; set; }

    [BsonElement("isMinorityOwned")] public bool IsMinorityOwned { get; set; }

    [BsonElement("isWomanOwned")] public bool IsWomanOwned { get; set; }

    [BsonElement("isVeteranOwned")] public bool IsVeteranOwned { get; set; }

    [BsonElement("isSmallBusiness")] public bool IsSmallBusiness { get; set; }

    // Product & Service Categories
    [BsonElement("productCategories")] public string? ProductCategories { get; set; }

    [BsonElement("serviceCategories")] public string? ServiceCategories { get; set; }

    [BsonElement("specialization")] public string? Specialization { get; set; }

    // Performance & Rating
    [BsonElement("qualityRating")] public int? QualityRating { get; set; }

    [BsonElement("deliveryRating")] public int? DeliveryRating { get; set; }

    [BsonElement("serviceRating")] public int? ServiceRating { get; set; }

    [BsonElement("priceRating")] public int? PriceRating { get; set; }

    [BsonElement("onTimeDeliveryPercentage")]
    public int? OnTimeDeliveryPercentage { get; set; }

    [BsonElement("defectRate")] public int? DefectRate { get; set; }

    // Relationship & Status
    [BsonElement("supplierStatus")] public string? SupplierStatus { get; set; }

    [BsonElement("isPreferred")] public bool IsPreferred { get; set; }

    [BsonElement("isApproved")] public bool IsApproved { get; set; } = true;

    [BsonElement("isActive")] public bool IsActive { get; set; } = true;

    [BsonElement("approvedDate")] public DateTime? ApprovedDate { get; set; }

    [BsonElement("approvedBy")] public string? ApprovedBy { get; set; }

    [BsonElement("partnerSince")] public DateTime? PartnerSince { get; set; }

    [BsonElement("lastOrderDate")] public DateTime? LastOrderDate { get; set; }

    [BsonElement("lastReviewDate")] public DateTime? LastReviewDate { get; set; }

    // Risk Assessment
    [BsonElement("riskLevel")] public string? RiskLevel { get; set; }

    [BsonElement("riskFactors")] public string? RiskFactors { get; set; }

    [BsonElement("requiresInsurance")] public bool RequiresInsurance { get; set; }

    [BsonElement("requiresBackgroundCheck")]
    public bool RequiresBackgroundCheck { get; set; }

    // Contract Information
    [BsonElement("contractNumber")] public string? ContractNumber { get; set; }

    [BsonElement("contractStartDate")] public DateTime? ContractStartDate { get; set; }

    [BsonElement("contractEndDate")] public DateTime? ContractEndDate { get; set; }

    [BsonElement("contractDocumentUrl")] public string? ContractDocumentUrl { get; set; }

    [BsonElement("autoRenew")] public bool AutoRenew { get; set; }

    // Shipping & Logistics
    [BsonElement("shippingMethods")] public string? ShippingMethods { get; set; }

    [BsonElement("shippingTerms")] public string? ShippingTerms { get; set; }

    [BsonElement("leadTimeDays")] public int? LeadTimeDays { get; set; }

    [BsonElement("minimumOrderValue")] public decimal? MinimumOrderValue { get; set; }

    [BsonElement("minimumOrderCurrency")] public string? MinimumOrderCurrency { get; set; }

    [BsonElement("dropShipCapable")] public bool DropShipCapable { get; set; }

    [BsonElement("warehouseLocations")] public string? WarehouseLocations { get; set; }

    // Support & Service
    [BsonElement("supportEmail")] public string? SupportEmail { get; set; }

    [BsonElement("supportPhone")] public string? SupportPhone { get; set; }

    [BsonElement("supportHours")] public string? SupportHours { get; set; }

    [BsonElement("slaDocumentUrl")] public string? SLADocumentUrl { get; set; }

    [BsonElement("provides24x7Support")] public bool Provides24x7Support { get; set; }

    // Insurance & Bonding
    [BsonElement("insuranceProvider")] public string? InsuranceProvider { get; set; }

    [BsonElement("insurancePolicyNumber")] public string? InsurancePolicyNumber { get; set; }

    [BsonElement("insuranceCoverage")] public decimal? InsuranceCoverage { get; set; }

    [BsonElement("insuranceExpiryDate")] public DateTime? InsuranceExpiryDate { get; set; }

    [BsonElement("bondingInformation")] public string? BondingInformation { get; set; }

    // Diversity & Sustainability
    [BsonElement("isMbe")] public bool IsMBE { get; set; }

    [BsonElement("isWbe")] public bool IsWBE { get; set; }

    [BsonElement("isSdvosb")] public bool IsSDVOSB { get; set; }

    [BsonElement("isEnvironmentallyCertified")]
    public bool IsEnvironmentallyCertified { get; set; }

    [BsonElement("sustainabilityRating")] public string? SustainabilityRating { get; set; }

    // Internal Management
    [BsonElement("internalNotes")] public string? InternalNotes { get; set; }

    [BsonElement("notes")] public string? Notes { get; set; }

    [BsonElement("procurementNotes")] public string? ProcurementNotes { get; set; }

    [BsonElement("ourAccountManager")] public string? OurAccountManager { get; set; }

    [BsonElement("tags")] public string? Tags { get; set; }

    // Statistics
    [BsonElement("totalOrders")] public int? TotalOrders { get; set; }

    [BsonElement("totalSpent")] public decimal? TotalSpent { get; set; }

    [BsonElement("totalSpentCurrency")] public string? TotalSpentCurrency { get; set; }

    [BsonElement("averageOrderValue")] public decimal? AverageOrderValue { get; set; }

    // Documents & Attachments
    [BsonElement("w9FormUrl")] public string? W9FormUrl { get; set; }

    [BsonElement("certificateOfInsuranceUrl")]
    public string? CertificateOfInsuranceUrl { get; set; }

    [BsonElement("businessLicenseUrl")] public string? BusinessLicenseUrl { get; set; }

    [BsonElement("references")] public string? References { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assetIds")] public List<long> AssetIds { get; set; } = new();

    public SupplierMongo()
    {
    }

    public SupplierMongo(long domainId) : base(domainId)
    {
    }
}