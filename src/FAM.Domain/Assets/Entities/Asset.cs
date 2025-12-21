using FAM.Domain.Assets.Events;
using FAM.Domain.Categories;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Conditions;
using FAM.Domain.Finance;
using FAM.Domain.Geography;
using FAM.Domain.Locations;
using FAM.Domain.Manufacturers;
using FAM.Domain.Models;
using FAM.Domain.Organizations;
using FAM.Domain.Statuses;
using FAM.Domain.Suppliers;
using FAM.Domain.Types;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Assets;

/// <summary>
/// Tài sản chính - Aggregate Root
/// Uses full audit trail with domain events
/// </summary>
public class Asset : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier, IHasDeletionTime,
    IHasDeleter, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // Domain fields
    public string Name { get; private set; } = string.Empty;

    // Company & Type
    public long? CompanyId { get; private set; }
    public long? AssetTypeId { get; private set; }
    public long? CategoryId { get; private set; }

    // Model & Manufacturer
    public long? ModelId { get; private set; }
    public long? ManufacturerId { get; private set; }

    // Identification
    public SerialNumber? SerialNo { get; private set; }
    public AssetTag? AssetTag { get; private set; }
    public string? Barcode { get; private set; }
    public string? QRCode { get; private set; }
    public string? RFIDTag { get; private set; }

    // Purchase Information
    public DateTime? PurchaseDate { get; private set; }
    public decimal? PurchaseCost { get; private set; }
    public string? PurchaseOrderNo { get; private set; }
    public string? InvoiceNo { get; private set; }
    public long? SupplierId { get; private set; }
    public DateTime? WarrantyUntil { get; private set; }
    public int? WarrantyMonths { get; private set; }
    public string? WarrantyTerms { get; private set; }

    // Condition & Location
    public long? ConditionId { get; private set; }
    public long? LocationId { get; private set; }
    public string? LocationCode { get; private set; }
    public long? CountryId { get; private set; }

    // Assignment
    public long? DepartmentId { get; private set; }
    public long? OwnerId { get; private set; }

    // Status
    public string LifecycleCode { get; private set; } = "draft";
    public string? UsageCode { get; private set; }

    // Depreciation & Finance
    public string? DepreciationMethod { get; private set; }
    public int? UsefulLifeMonths { get; private set; }
    public decimal? ResidualValue { get; private set; }
    public DateTime? InServiceDate { get; private set; }
    public decimal? CurrentBookValue { get; private set; }
    public decimal? AccumulatedDepreciation { get; private set; }
    public DateTime? LastDepreciationDate { get; private set; }
    public string? AccountingCode { get; private set; }
    public string? CostCenter { get; private set; }
    public string? GLAccount { get; private set; }

    // Insurance & Risk
    public string? InsurancePolicyNo { get; private set; }
    public decimal? InsuredValue { get; private set; }
    public DateTime? InsuranceExpiryDate { get; private set; }
    public string? RiskLevel { get; private set; } // Low, Medium, High, Critical

    // Maintenance & Support
    public DateTime? LastMaintenanceDate { get; private set; }
    public DateTime? NextMaintenanceDate { get; private set; }
    public int? MaintenanceIntervalDays { get; private set; }
    public string? MaintenanceContractNo { get; private set; }
    public DateTime? SupportExpiryDate { get; private set; }
    public string? ServiceLevel { get; private set; } // Bronze, Silver, Gold, Platinum

    // IT/Software Specific (if applicable)
    public IPAddress? IPAddress { get; private set; }
    public MACAddress? MACAddress { get; private set; }
    public string? Hostname { get; private set; }
    public string? OperatingSystem { get; private set; }
    public string? SoftwareVersion { get; private set; }
    public string? LicenseKey { get; private set; }
    public DateTime? LicenseExpiryDate { get; private set; }
    public int? LicenseCount { get; private set; }

    // Physical Characteristics
    public decimal? Weight { get; private set; } // kg
    public string? Dimensions { get; private set; } // "L x W x H" in cm
    public string? Color { get; private set; }
    public string? Material { get; private set; }

    // Energy & Environmental
    public decimal? PowerConsumption { get; private set; } // Watts
    public string? EnergyRating { get; private set; } // A+, A, B, C, etc.
    public bool IsEnvironmentallyFriendly { get; private set; }
    public DateTime? EndOfLifeDate { get; private set; }
    public string? DisposalMethod { get; private set; }

    // Compliance & Security
    public string? ComplianceStatus { get; private set; } // Compliant, NonCompliant, UnderReview
    public string? SecurityClassification { get; private set; } // Public, Internal, Confidential, Secret
    public bool RequiresBackgroundCheck { get; private set; }
    public string? DataClassification { get; private set; }
    public DateTime? LastAuditDate { get; private set; }
    public DateTime? NextAuditDate { get; private set; }

    // Additional tracking
    public string? ProjectCode { get; private set; }
    public string? CampaignCode { get; private set; }
    public string? FundingSource { get; private set; }
    public decimal? ReplacementCost { get; private set; }
    public int? EstimatedRemainingLifeMonths { get; private set; }
    public string? Notes { get; private set; }
    public string? InternalNotes { get; private set; } // Private notes, not visible to all users

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Navigation properties
    /// <summary>
    /// Assignment records for this asset (part of Asset aggregate)
    /// </summary>
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    /// <summary>
    /// Lifecycle events for this asset (part of Asset aggregate)
    /// </summary>
    public ICollection<AssetEvent> AssetEvents { get; set; } = new List<AssetEvent>();

    /// <summary>
    /// Finance entries (depreciation, adjustments) for this asset (part of Asset aggregate)
    /// </summary>
    public ICollection<FinanceEntry> FinanceEntries { get; set; } = new List<FinanceEntry>();

    /// <summary>
    /// Attachments/documents for this asset (part of Asset aggregate)
    /// </summary>
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    // Reference-only navigations (NOT included in aggregate boundary):
    public AssetType? AssetType { get; set; }
    public AssetCategory? Category { get; set; }
    public Model? Model { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public Supplier? Supplier { get; set; }
    public AssetCondition? Condition { get; set; }
    public Location? Location { get; set; }
    public Country? Country { get; set; }
    public CompanyDetails? Company { get; set; }
    public User? Owner { get; set; }
    public LifecycleStatus? LifecycleStatus { get; set; }
    public UsageStatus? UsageStatus { get; set; }

    // EF Core constructor
    private Asset()
    {
    }

    // Factory method
    public static Asset Create(
        string name,
        long createdById,
        int? companyId = null,
        int? assetTypeId = null,
        string? assetTag = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Asset name cannot be empty");

        if (createdById <= 0)
            throw new DomainException("CreatedBy must be a positive number");

        var asset = new Asset
        {
            Name = name.Trim(),
            CompanyId = companyId,
            AssetTypeId = assetTypeId,
            AssetTag = assetTag != null ? AssetTag.Create(assetTag) : null,
            LifecycleCode = "draft",
            CreatedAt = DateTime.UtcNow,
            CreatedById = createdById
        };

        asset.RaiseDomainEvent(new AssetCreated
        {
            AssetId = asset.Id,
            AssetName = name,
            AssetTag = assetTag,
            CompanyId = companyId
        });

        return asset;
    }

    // Business methods
    public void UpdateBasicInfo(string name, string? notes, long updatedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Asset name cannot be empty");

        if (updatedById <= 0)
            throw new DomainException("UpdatedBy must be a positive number");

        Name = name.Trim();
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;

        RaiseDomainEvent(new AssetUpdated { AssetId = Id });
    }

    public void SetPurchaseInfo(DateTime? purchaseDate, decimal? purchaseCost, int? supplierId, long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (purchaseCost.HasValue && purchaseCost.Value < 0)
            throw new DomainException("Purchase cost cannot be negative");

        PurchaseDate = purchaseDate;
        PurchaseCost = purchaseCost;
        SupplierId = supplierId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;

        RaiseDomainEvent(new AssetUpdated { AssetId = Id, PropertyChanged = "PurchaseInfo" });
    }

    public void SetLocation(int? locationId, string? locationCode, int? countryId, long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        LocationId = locationId;
        LocationCode = locationCode;
        CountryId = countryId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;

        RaiseDomainEvent(new AssetUpdated { AssetId = Id, PropertyChanged = "Location" });
    }

    public void Assign(string assigneeType, int assigneeId, long byUserId)
    {
        if (byUserId <= 0)
            throw new DomainException("ByUserId must be a positive number");

        if (string.IsNullOrWhiteSpace(assigneeType))
            throw new DomainException("AssigneeType cannot be empty");

        UsageCode = "in_use";
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = byUserId;

        RaiseDomainEvent(new AssetAssigned
        {
            AssetId = Id,
            AssigneeType = assigneeType,
            AssigneeId = assigneeId,
            ByUserId = byUserId
        });
    }

    public void Release(int assignmentId, long byUserId)
    {
        if (byUserId <= 0)
            throw new DomainException("ByUserId must be a positive number");

        UsageCode = "available";
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = byUserId;

        RaiseDomainEvent(new AssetReleased
        {
            AssetId = Id,
            AssignmentId = assignmentId,
            ByUserId = byUserId
        });
    }

    public void ChangeLifecycleStatus(string newStatus, long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new DomainException("New status cannot be empty");

        var oldStatus = LifecycleCode;
        LifecycleCode = newStatus;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;

        RaiseDomainEvent(new AssetUpdated { AssetId = Id, PropertyChanged = $"Lifecycle: {oldStatus} -> {newStatus}" });
    }

    public void Delete(string reason, long deletedById)
    {
        if (deletedById <= 0)
            throw new DomainException("DeletedById must be a positive number");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Deletion reason cannot be empty");

        SoftDelete(deletedById);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AssetSoftDeleted
        {
            AssetId = Id,
            Reason = reason,
            DeletedBy = deletedById
        });
    }

    public void RestoreAsset(long restoredById)
    {
        if (restoredById <= 0)
            throw new DomainException("RestoredById must be a positive number");

        Restore();
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = restoredById;

        RaiseDomainEvent(new AssetRestored
        {
            AssetId = Id,
            RestoredBy = restoredById
        });
    }

    // Identification & Tracking Methods
    public void SetIdentification(
        string? serialNo,
        long updatedById,
        string? barcode = null,
        string? qrCode = null,
        string? rfidTag = null)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        // Validate Barcode
        if (barcode != null)
        {
            barcode = barcode.Trim();
            if (string.IsNullOrEmpty(barcode))
                throw new DomainException("Barcode cannot be empty");
            if (barcode.Length > 50)
                throw new DomainException("Barcode cannot exceed 50 characters");
        }

        // Validate QRCode
        if (qrCode != null)
        {
            qrCode = qrCode.Trim();
            if (string.IsNullOrEmpty(qrCode))
                throw new DomainException("QR code cannot be empty");
            if (qrCode.Length > 100)
                throw new DomainException("QR code cannot exceed 100 characters");
        }

        // Validate RFIDTag
        if (rfidTag != null)
        {
            rfidTag = rfidTag.Trim();
            if (string.IsNullOrEmpty(rfidTag))
                throw new DomainException("RFID tag cannot be empty");
            if (rfidTag.Length > 50)
                throw new DomainException("RFID tag cannot exceed 50 characters");
        }

        SerialNo = serialNo != null ? SerialNumber.Create(serialNo) : null;
        Barcode = barcode;
        QRCode = qrCode;
        RFIDTag = rfidTag;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;

        RaiseDomainEvent(new AssetUpdated { AssetId = Id, PropertyChanged = "Identification" });
    }

    // Extended Purchase Information
    public void SetExtendedPurchaseInfo(
        string? purchaseOrderNo,
        string? invoiceNo,
        int? warrantyMonths,
        string? warrantyTerms,
        long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (warrantyMonths.HasValue && warrantyMonths.Value < 0)
            throw new DomainException("Warranty months cannot be negative");

        // Validate PurchaseOrderNo
        if (purchaseOrderNo != null)
        {
            purchaseOrderNo = purchaseOrderNo.Trim();
            if (string.IsNullOrEmpty(purchaseOrderNo))
                throw new DomainException("Purchase order number cannot be empty");
            if (purchaseOrderNo.Length > 50)
                throw new DomainException("Purchase order number cannot exceed 50 characters");
        }

        // Validate InvoiceNo
        if (invoiceNo != null)
        {
            invoiceNo = invoiceNo.Trim();
            if (string.IsNullOrEmpty(invoiceNo))
                throw new DomainException("Invoice number cannot be empty");
            if (invoiceNo.Length > 50)
                throw new DomainException("Invoice number cannot exceed 50 characters");
        }

        PurchaseOrderNo = purchaseOrderNo;
        InvoiceNo = invoiceNo;
        WarrantyMonths = warrantyMonths;
        WarrantyTerms = warrantyTerms;

        if (warrantyMonths.HasValue && PurchaseDate.HasValue)
            WarrantyUntil = PurchaseDate.Value.AddMonths(warrantyMonths.Value);

        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    // Finance & Accounting
    public void SetFinanceInfo(
        string? accountingCode,
        string? costCenter,
        string? glAccount,
        long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        // Validate AccountingCode
        if (accountingCode != null)
        {
            accountingCode = accountingCode.Trim();
            if (string.IsNullOrEmpty(accountingCode))
                throw new DomainException("Accounting code cannot be empty");
            if (accountingCode.Length > 30)
                throw new DomainException("Accounting code cannot exceed 30 characters");
        }

        // Validate CostCenter
        if (costCenter != null)
        {
            costCenter = costCenter.Trim();
            if (string.IsNullOrEmpty(costCenter))
                throw new DomainException("Cost center cannot be empty");
            if (costCenter.Length > 30)
                throw new DomainException("Cost center cannot exceed 30 characters");
        }

        // Validate GLAccount
        if (glAccount != null)
        {
            glAccount = glAccount.Trim();
            if (string.IsNullOrEmpty(glAccount))
                throw new DomainException("GL account cannot be empty");
            if (glAccount.Length > 30)
                throw new DomainException("GL account cannot exceed 30 characters");
        }

        AccountingCode = accountingCode;
        CostCenter = costCenter;
        GLAccount = glAccount;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public void UpdateDepreciation(
        decimal currentBookValue,
        decimal accumulatedDepreciation,
        long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (currentBookValue < 0)
            throw new DomainException("Current book value cannot be negative");

        if (accumulatedDepreciation < 0)
            throw new DomainException("Accumulated depreciation cannot be negative");

        CurrentBookValue = currentBookValue;
        AccumulatedDepreciation = accumulatedDepreciation;
        LastDepreciationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsFullyDepreciated()
    {
        if (!CurrentBookValue.HasValue || !ResidualValue.HasValue)
            return false;

        return CurrentBookValue.Value <= ResidualValue.Value;
    }

    // Insurance Management
    public void SetInsurance(
        string? policyNo,
        decimal? insuredValue,
        DateTime? expiryDate,
        long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        // Validate PolicyNo
        if (policyNo != null)
        {
            policyNo = policyNo.Trim();
            if (string.IsNullOrEmpty(policyNo))
                throw new DomainException("Insurance policy number cannot be empty");
            if (policyNo.Length > 50)
                throw new DomainException("Insurance policy number cannot exceed 50 characters");
        }

        InsurancePolicyNo = policyNo;
        InsuredValue = insuredValue;
        InsuranceExpiryDate = expiryDate;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsInsuranceExpired()
    {
        return InsuranceExpiryDate.HasValue && InsuranceExpiryDate.Value < DateTime.UtcNow;
    }

    public bool IsInsuranceExpiringSoon(int daysThreshold = 30)
    {
        return InsuranceExpiryDate.HasValue &&
               InsuranceExpiryDate.Value <= DateTime.UtcNow.AddDays(daysThreshold) &&
               InsuranceExpiryDate.Value > DateTime.UtcNow;
    }

    // Risk Management
    public void SetRiskLevel(string riskLevel, long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (!IsValidRiskLevel(riskLevel))
            throw new DomainException($"Invalid risk level: {riskLevel}. Must be Low, Medium, High, or Critical.");

        RiskLevel = riskLevel;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    private static bool IsValidRiskLevel(string level)
    {
        return new[] { "Low", "Medium", "High", "Critical" }.Contains(level);
    }

    public bool IsCriticalAsset()
    {
        return RiskLevel == "Critical";
    }

    // Maintenance Management
    public void ScheduleMaintenance(
        int intervalDays,
        long updatedById,
        DateTime? nextMaintenanceDate = null,
        string? contractNo = null)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (intervalDays <= 0)
            throw new DomainException("Maintenance interval must be positive");

        // Validate ContractNo
        if (contractNo != null)
        {
            contractNo = contractNo.Trim();
            if (string.IsNullOrEmpty(contractNo))
                throw new DomainException("Maintenance contract number cannot be empty");
            if (contractNo.Length > 50)
                throw new DomainException("Maintenance contract number cannot exceed 50 characters");
        }

        MaintenanceIntervalDays = intervalDays;
        MaintenanceContractNo = contractNo;
        NextMaintenanceDate = nextMaintenanceDate ?? DateTime.UtcNow.AddDays(intervalDays);
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public void RecordMaintenance(DateTime maintenanceDate, long updatedById)
    {
        if (updatedById <= 0)
            throw new DomainException("UpdatedById must be a positive number");

        if (maintenanceDate > DateTime.UtcNow)
            throw new DomainException("Maintenance date cannot be in the future");

        LastMaintenanceDate = maintenanceDate;

        if (MaintenanceIntervalDays.HasValue)
            NextMaintenanceDate = maintenanceDate.AddDays(MaintenanceIntervalDays.Value);

        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsMaintenanceDue()
    {
        return NextMaintenanceDate.HasValue && NextMaintenanceDate.Value <= DateTime.UtcNow;
    }

    public bool IsMaintenanceOverdue()
    {
        if (!NextMaintenanceDate.HasValue)
            return false;

        return NextMaintenanceDate.Value < DateTime.UtcNow;
    }

    public int? DaysUntilMaintenance()
    {
        return NextMaintenanceDate.HasValue
            ? (int?)(NextMaintenanceDate.Value - DateTime.UtcNow).Days
            : null;
    }

    // Support & Service Level
    public void SetSupport(
        string? serviceLevel,
        DateTime? supportExpiryDate,
        long? updatedById = null)
    {
        if (serviceLevel != null && !IsValidServiceLevel(serviceLevel))
            throw new DomainException(
                $"Invalid service level: {serviceLevel}. Must be Bronze, Silver, Gold, or Platinum.");

        ServiceLevel = serviceLevel;
        SupportExpiryDate = supportExpiryDate;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    private static bool IsValidServiceLevel(string level)
    {
        return new[] { "Bronze", "Silver", "Gold", "Platinum" }.Contains(level);
    }

    public bool IsSupportExpired()
    {
        return SupportExpiryDate.HasValue && SupportExpiryDate.Value < DateTime.UtcNow;
    }

    // IT Asset Management
    public void SetITInfo(
        string? ipAddress,
        string? macAddress,
        string? hostname,
        string? operatingSystem,
        long? updatedById = null)
    {
        // Validate Hostname
        if (hostname != null)
        {
            hostname = hostname.Trim();
            if (string.IsNullOrEmpty(hostname))
                throw new DomainException("Hostname cannot be empty");
            if (hostname.Length > 100)
                throw new DomainException("Hostname cannot exceed 100 characters");
            if (hostname.Contains(" "))
                throw new DomainException("Hostname cannot contain spaces");
        }

        IPAddress = ipAddress != null ? IPAddress.Create(ipAddress) : null;
        MACAddress = macAddress != null ? MACAddress.Create(macAddress) : null;
        Hostname = hostname;
        OperatingSystem = operatingSystem;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public void SetSoftwareInfo(
        string? version,
        string? licenseKey,
        DateTime? licenseExpiry,
        int? licenseCount = null,
        long? updatedById = null)
    {
        if (licenseCount.HasValue && licenseCount.Value < 0)
            throw new DomainException("License count cannot be negative");

        // Validate LicenseKey
        if (licenseKey != null)
        {
            licenseKey = licenseKey.Trim();
            if (string.IsNullOrEmpty(licenseKey))
                throw new DomainException("License key cannot be empty");
            if (licenseKey.Length > 100)
                throw new DomainException("License key cannot exceed 100 characters");
        }

        SoftwareVersion = version;
        LicenseKey = licenseKey;
        LicenseExpiryDate = licenseExpiry;
        LicenseCount = licenseCount;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsLicenseExpired()
    {
        return LicenseExpiryDate.HasValue && LicenseExpiryDate.Value < DateTime.UtcNow;
    }

    public bool IsLicenseExpiringSoon(int daysThreshold = 30)
    {
        return LicenseExpiryDate.HasValue &&
               LicenseExpiryDate.Value <= DateTime.UtcNow.AddDays(daysThreshold) &&
               LicenseExpiryDate.Value > DateTime.UtcNow;
    }

    // Physical Characteristics
    public void SetPhysicalCharacteristics(
        decimal? weight,
        string? dimensions,
        string? color,
        string? material,
        long? updatedById = null)
    {
        if (weight.HasValue && weight.Value < 0)
            throw new DomainException("Weight cannot be negative");

        Weight = weight;
        Dimensions = dimensions;
        Color = color;
        Material = material;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    // Environmental & Energy
    public void SetEnvironmentalInfo(
        decimal? powerConsumption,
        string? energyRating,
        bool isEnvironmentallyFriendly,
        long? updatedById = null)
    {
        if (powerConsumption.HasValue && powerConsumption.Value < 0)
            throw new DomainException("Power consumption cannot be negative");

        PowerConsumption = powerConsumption;
        EnergyRating = energyRating;
        IsEnvironmentallyFriendly = isEnvironmentallyFriendly;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public void SetEndOfLife(
        DateTime endOfLifeDate,
        string disposalMethod,
        long? updatedById = null)
    {
        if (string.IsNullOrWhiteSpace(disposalMethod))
            throw new DomainException("Disposal method cannot be empty");

        if (endOfLifeDate < DateTime.UtcNow.AddYears(-10) || endOfLifeDate > DateTime.UtcNow.AddYears(50))
            throw new DomainException("End of life date seems unreasonable");

        EndOfLifeDate = endOfLifeDate;
        DisposalMethod = disposalMethod;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsNearingEndOfLife(int daysThreshold = 90)
    {
        return EndOfLifeDate.HasValue &&
               EndOfLifeDate.Value <= DateTime.UtcNow.AddDays(daysThreshold) &&
               EndOfLifeDate.Value > DateTime.UtcNow;
    }

    // Compliance & Security
    public void SetCompliance(
        string? complianceStatus,
        DateTime? lastAuditDate,
        DateTime? nextAuditDate,
        long? updatedById = null)
    {
        ComplianceStatus = complianceStatus;
        LastAuditDate = lastAuditDate;
        NextAuditDate = nextAuditDate;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public void SetSecurity(
        string? securityClassification,
        bool requiresBackgroundCheck,
        string? dataClassification,
        long? updatedById = null)
    {
        SecurityClassification = securityClassification;
        RequiresBackgroundCheck = requiresBackgroundCheck;
        DataClassification = dataClassification;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsAuditDue()
    {
        return NextAuditDate.HasValue && NextAuditDate.Value <= DateTime.UtcNow;
    }

    public bool IsHighSecurity()
    {
        return SecurityClassification is "Confidential" or "Secret";
    }

    // Project & Campaign Tracking
    public void SetProjectInfo(
        string? projectCode,
        string? campaignCode,
        string? fundingSource,
        long? updatedById = null)
    {
        // Validate ProjectCode
        if (projectCode != null)
        {
            projectCode = projectCode.Trim();
            if (string.IsNullOrEmpty(projectCode))
                throw new DomainException("Project code cannot be empty");
            if (projectCode.Length > 30)
                throw new DomainException("Project code cannot exceed 30 characters");
        }

        // Validate CampaignCode
        if (campaignCode != null)
        {
            campaignCode = campaignCode.Trim();
            if (string.IsNullOrEmpty(campaignCode))
                throw new DomainException("Campaign code cannot be empty");
            if (campaignCode.Length > 30)
                throw new DomainException("Campaign code cannot exceed 30 characters");
        }

        ProjectCode = projectCode;
        CampaignCode = campaignCode;
        FundingSource = fundingSource;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    // Replacement Planning
    public void SetReplacementInfo(
        decimal? replacementCost,
        int? estimatedRemainingLifeMonths,
        long? updatedById = null)
    {
        if (replacementCost.HasValue && replacementCost.Value < 0)
            throw new DomainException("Replacement cost cannot be negative");

        if (estimatedRemainingLifeMonths.HasValue && estimatedRemainingLifeMonths.Value < 0)
            throw new DomainException("Estimated remaining life months cannot be negative");

        ReplacementCost = replacementCost;
        EstimatedRemainingLifeMonths = estimatedRemainingLifeMonths;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById;
    }

    public bool IsReplacementDue()
    {
        return EstimatedRemainingLifeMonths.HasValue && EstimatedRemainingLifeMonths.Value <= 0;
    }

    public bool IsReplacementSoon(int monthsThreshold = 6)
    {
        return EstimatedRemainingLifeMonths.HasValue &&
               EstimatedRemainingLifeMonths.Value <= monthsThreshold &&
               EstimatedRemainingLifeMonths.Value > 0;
    }

    // Comprehensive Status Check
    public AssetHealthStatus GetHealthStatus()
    {
        var issues = new List<string>();

        if (IsMaintenanceOverdue())
            issues.Add("Maintenance overdue");

        if (IsWarrantyExpired())
            issues.Add("Warranty expired");

        if (IsInsuranceExpired())
            issues.Add("Insurance expired");

        if (IsLicenseExpired())
            issues.Add("License expired");

        if (IsSupportExpired())
            issues.Add("Support expired");

        if (IsAuditDue())
            issues.Add("Audit due");

        if (IsReplacementDue())
            issues.Add("Replacement due");

        if (issues.Count == 0)
            return AssetHealthStatus.Healthy;

        if (issues.Count <= 2)
            return AssetHealthStatus.NeedsAttention;

        return AssetHealthStatus.Critical;
    }

    public bool IsWarrantyExpired()
    {
        return WarrantyUntil.HasValue && WarrantyUntil.Value < DateTime.UtcNow;
    }

    public bool IsWarrantyActive()
    {
        return WarrantyUntil.HasValue && WarrantyUntil.Value >= DateTime.UtcNow;
    }

    public int? DaysUntilWarrantyExpiry()
    {
        return WarrantyUntil.HasValue
            ? (int?)(WarrantyUntil.Value - DateTime.UtcNow).Days
            : null;
    }

    // Domain event methods
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // Audit methods
    public void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum AssetHealthStatus
{
    Healthy,
    NeedsAttention,
    Critical
}
