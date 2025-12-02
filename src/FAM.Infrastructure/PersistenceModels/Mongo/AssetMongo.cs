using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for Asset
/// </summary>
[BsonIgnoreExtraElements]
public class AssetMongo : BaseEntityMongo
{
    // Basic Information
    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    // Company & Type
    [BsonElement("companyId")] public long? CompanyId { get; set; }

    [BsonElement("assetTypeId")] public long? AssetTypeId { get; set; }

    [BsonElement("categoryId")] public long? CategoryId { get; set; }

    // Model & Manufacturer
    [BsonElement("modelId")] public long? ModelId { get; set; }

    [BsonElement("manufacturerId")] public long? ManufacturerId { get; set; }

    // Identification
    [BsonElement("serialNo")] public string? SerialNo { get; set; }

    [BsonElement("assetTag")] public string? AssetTag { get; set; }

    [BsonElement("barcode")] public string? Barcode { get; set; }

    [BsonElement("qrCode")] public string? QRCode { get; set; }

    [BsonElement("rfidTag")] public string? RFIDTag { get; set; }

    // Purchase Information
    [BsonElement("purchaseDate")] public DateTime? PurchaseDate { get; set; }

    [BsonElement("purchaseCost")] public decimal? PurchaseCost { get; set; }

    [BsonElement("purchaseOrderNo")] public string? PurchaseOrderNo { get; set; }

    [BsonElement("invoiceNo")] public string? InvoiceNo { get; set; }

    [BsonElement("supplierId")] public long? SupplierId { get; set; }

    [BsonElement("warrantyUntil")] public DateTime? WarrantyUntil { get; set; }

    [BsonElement("warrantyMonths")] public int? WarrantyMonths { get; set; }

    [BsonElement("warrantyTerms")] public string? WarrantyTerms { get; set; }

    // Condition & Location
    [BsonElement("conditionId")] public long? ConditionId { get; set; }

    [BsonElement("locationId")] public long? LocationId { get; set; }

    [BsonElement("locationCode")] public string? LocationCode { get; set; }

    [BsonElement("countryId")] public long? CountryId { get; set; }

    // Assignment
    [BsonElement("departmentId")] public long? DepartmentId { get; set; }

    [BsonElement("ownerId")] public long? OwnerId { get; set; }

    // Status
    [BsonElement("usageStatusId")] public long? UsageStatusId { get; set; }

    [BsonElement("lifecycleStatusId")] public long? LifecycleStatusId { get; set; }

    // Financial
    [BsonElement("currentValue")] public decimal? CurrentValue { get; set; }

    [BsonElement("accumulatedDepreciation")]
    public decimal? AccumulatedDepreciation { get; set; }

    [BsonElement("netBookValue")] public decimal? NetBookValue { get; set; }

    // Maintenance
    [BsonElement("lastMaintenanceDate")] public DateTime? LastMaintenanceDate { get; set; }

    [BsonElement("nextMaintenanceDate")] public DateTime? NextMaintenanceDate { get; set; }

    [BsonElement("maintenanceIntervalDays")]
    public int? MaintenanceIntervalDays { get; set; }

    // Custom Fields
    [BsonElement("customFields")] public string? CustomFields { get; set; }

    // Notes
    [BsonElement("notes")] public string? Notes { get; set; }

    // Audit
    [BsonElement("createdBy")] public long? CreatedBy { get; set; }

    [BsonElement("updatedBy")] public long? UpdatedBy { get; set; }

    // Navigation properties (stored as references)
    [BsonElement("assignmentIds")] public List<long> AssignmentIds { get; set; } = new();

    [BsonElement("assetEventIds")] public List<long> AssetEventIds { get; set; } = new();

    [BsonElement("attachmentIds")] public List<long> AttachmentIds { get; set; } = new();

    [BsonElement("financeEntryIds")] public List<long> FinanceEntryIds { get; set; } = new();

    public AssetMongo()
    {
    }

    public AssetMongo(long domainId) : base(domainId)
    {
    }
}