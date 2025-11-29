# FAM.Domain - Domain Layer

# FAM.Domain - Domain Layer

Domain layer cho hệ thống Fixed Asset Management, được thiết kế theo Domain-Driven Design (DDD) principles với đầy đủ
các tactical patterns và enterprise asset management features.

## 📋 Tổng Quan

Domain layer chứa toàn bộ business logic và domain knowledge của hệ thống quản lý tài sản doanh nghiệp. Được tổ chức
theo DDD patterns với:

- **Aggregate Roots**: Asset, Location
- **Entities**: 20+ domain entities
- **Value Objects**: 11 value objects với business logic
- **Domain Services**: 8 domain services
- **Specifications**: 24+ reusable query specifications
- **Domain Events**: 13 domain events
- **Repository Interfaces**: Generic + Specific repositories với 50+ query methods

## 📁 Cấu Trúc Thư Mục

```
FAM.Domain/
├── Common/                          # Base classes
│   ├── Entity.cs                   # Base entity với soft delete
│   ├── AggregateRoot.cs            # Base cho aggregate roots
│   ├── ValueObject.cs              # Base cho value objects
│   ├── Enumeration.cs              # Base cho enumerations
│   └── IDomainEvent.cs             # Interface cho domain events
│
├── ValueObjects/                    # 11 Value Objects
│   ├── Money.cs                    # Tiền tệ với currency
│   ├── Address.cs                  # Địa chỉ đầy đủ
│   ├── DateRange.cs                # Khoảng thời gian
│   ├── Email.cs                    # Email validated
│   ├── PhoneNumber.cs              # Số điện thoại
│   ├── DepreciationInfo.cs         # Thông tin khấu hao
│   ├── AssetId.cs                  # ID tài sản
│   ├── WarrantyInfo.cs             # Thông tin bảo hành
│   ├── InsuranceInfo.cs            # Thông tin bảo hiểm
│   ├── IPAddress.cs                # IP address validated
│   └── Dimensions.cs               # Kích thước vật lý
│
├── Assets/                          # Asset Aggregate
│   ├── Asset.cs                    # Aggregate Root với 60+ fields, 30+ methods
│   ├── Assignment.cs               # Bàn giao tài sản
│   ├── AssetEvent.cs              # Sự kiện tài sản
│   ├── Attachment.cs              # File đính kèm
│   │
│   ├── Events/                     # 13 Domain Events
│   │   ├── AssetCreated.cs
│   │   ├── AssetUpdated.cs
│   │   ├── AssetDeleted.cs
│   │   ├── AssetSoftDeleted.cs
│   │   ├── AssetRestored.cs
│   │   ├── AssetAssigned.cs
│   │   ├── AssetReleased.cs
│   │   ├── AssetMaintenancePerformed.cs
│   │   ├── AssetExpirationWarning.cs
│   │   ├── AssetReplacementRequired.cs
│   │   ├── AssetDepreciationCalculated.cs
│   │   ├── AssetAudited.cs
│   │   └── AssetConditionChanged.cs
│   │
│   └── Specifications/             # 24 Specifications
│       ├── AssetSpecifications.cs  # 8 basic specifications
│       └── ExtendedAssetSpecifications.cs  # 16 extended specs
│
├── Types/                          # Classification entities
│   ├── AssetType.cs
│   ├── Model.cs
│   └── Manufacturer.cs
│
├── Categories/
│   └── AssetCategory.cs
│
├── Locations/                      # Location Aggregate
│   ├── Location.cs                 # Aggregate Root
│   ├── Building.cs
│   ├── Floor.cs
│   └── Room.cs
│
├── Geography/
│   ├── Country.cs
│   ├── Region.cs
│   ├── City.cs
│   └── District.cs
│
├── Organizations/
│   ├── Company.cs
│   ├── Department.cs
│   └── Supplier.cs
│
├── Finance/
│   ├── AssetCondition.cs
│   ├── FinanceEntry.cs
│   └── DepreciationHistory.cs
│
├── Accounts/
│   └── User.cs
│
├── Statuses/
│   ├── LifecycleStatus.cs
│   └── UsageStatus.cs
│
├── Services/                        # 8 Domain Services
│   ├── IDepreciationService.cs
│   ├── IAssetTagGenerator.cs
│   ├── ILocationHierarchyService.cs
│   ├── IAssetLifecycleValidator.cs
│   ├── IInsuranceCalculator.cs
│   ├── IMaintenanceScheduler.cs
│   ├── IAssetLifecycleManager.cs
│   └── IComplianceManager.cs
│
└── Abstractions/                    # Repository & Specifications
    ├── IRepository.cs              # Generic repository
    ├── IAssetRepository.cs         # 50+ query methods
    ├── ILocationRepository.cs
    ├── IUnitOfWork.cs
    └── ISpecification.cs

```

## 🎯 Tính Năng Chính

### 1. Enterprise Asset Management

Asset entity được thiết kế theo chuẩn doanh nghiệp với:

#### Identification & Tracking

- **Basic**: AssetTag, SerialNo, Name
- **Extended**: Barcode, QRCode, RFIDTag
- **Purchase**: PO Number, Invoice, Supplier

#### Financial Management

- **Acquisition**: PurchaseCost, PurchaseDate, Supplier
- **Depreciation**: CurrentBookValue, AccumulatedDepreciation, DepreciationMethod
- **Accounting**: AccountingCode, CostCenter, GLAccount
- **Residual**: ResidualValue, EstimatedValue

#### Insurance & Risk

- **Insurance**: PolicyNo, InsuredValue, ExpiryDate, Provider
- **Risk**: RiskLevel (Low/Medium/High/Critical)
- **Coverage**: CoverageType, Premium estimation

#### Maintenance & Support

- **Schedule**: LastMaintenanceDate, NextMaintenanceDate, IntervalDays
- **Contract**: MaintenanceContractNo, ServiceLevel
- **Support**: SupportExpiryDate, WarrantyTerms

#### IT Asset Management

- **Network**: IPAddress, MACAddress, Hostname
- **Software**: OS, SoftwareVersion, LicenseKey
- **Licensing**: LicenseExpiryDate, LicenseCount

#### Physical Characteristics

- **Dimensions**: Weight, Dimensions (L x W x H)
- **Appearance**: Color, Material
- **Energy**: PowerConsumption, EnergyRating

#### Environmental & Sustainability

- **Eco**: IsEnvironmentallyFriendly
- **Lifecycle**: EndOfLifeDate, DisposalMethod
- **Compliance**: Environmental standards

#### Compliance & Security

- **Compliance**: ComplianceStatus (Compliant/NonCompliant)
- **Security**: SecurityClassification (Public/Internal/Confidential/Secret)
- **Access**: RequiresBackgroundCheck, DataClassification
- **Audit**: LastAuditDate, NextAuditDate

#### Project & Tracking

- **Project**: ProjectCode, CampaignCode
- **Funding**: FundingSource
- **Replacement**: ReplacementCost, EstimatedRemainingLifeMonths

### 2. Soft Delete Pattern

Tất cả entities kế thừa từ `Entity` base class với soft delete support:

```csharp
public abstract class Entity
{
    // Soft delete fields
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }

    // Methods
    public void SoftDelete(string deletedBy)
    public void Restore()
}
```

### 3. Value Objects (11 VOs)

**Money** - Tiền tệ với operations

```csharp
var price = Money.Create(1000, "USD");
var discount = price * 0.1m;
var total = price - discount;
```

**Address** - Địa chỉ đầy đủ

```csharp
var address = Address.Create("123 Main St", "Hanoi", "Vietnam", "100000");
var isLocal = address.IsInCountry("Vietnam");
```

**WarrantyInfo** - Bảo hành

```csharp
var warranty = WarrantyInfo.Create(purchaseDate, 24, terms: "Full coverage");
var isActive = warranty.IsActive();
var daysLeft = warranty.DaysRemaining();
```

**InsuranceInfo** - Bảo hiểm

```csharp
var insurance = InsuranceInfo.Create("POL123", 50000, expiryDate);
var needsRenewal = insurance.IsExpiringSoon(30);
```

**IPAddress** - IP validated

```csharp
var ip = IPAddress.Create("192.168.1.100");
var isPrivate = ip.IsPrivate();
```

**Dimensions** - Kích thước

```csharp
var dims = Dimensions.Create(50, 30, 20, "cm");
var volume = dims.Volume();
var inMeters = dims.ConvertTo("m");
```

### 4. Domain Services (8 Services)

#### IDepreciationService

Tính toán khấu hao theo các phương pháp:

- Straight Line
- Declining Balance
- Double Declining Balance
- Sum of Years Digits

#### IInsuranceCalculator

- Tính giá trị bảo hiểm đề xuất
- Estimate phí bảo hiểm
- Đánh giá risk level
- Lấy assets cần renew

#### IMaintenanceScheduler

- Lập lịch bảo trì định kỳ
- Tính next maintenance date
- Identify urgent maintenance
- Generate maintenance plan
- Estimate costs

#### IAssetLifecycleManager

- Tính remaining life
- Identify end-of-life assets
- Evaluate asset health
- Recommend next actions
- Calculate Total Cost of Ownership (TCO)

#### IComplianceManager

- Check compliance
- Get non-compliant assets
- Schedule audits
- Get security requirements
- Generate compliance reports

### 5. Specifications Pattern (24+ Specs)

**Basic Specifications:**

- `ActiveAssetSpecification` - Assets chưa xóa
- `AssetByCompanySpecification` - Theo công ty
- `AvailableAssetSpecification` - Sẵn sàng bàn giao
- `InUseAssetSpecification` - Đang sử dụng
- `AssetNeedingDepreciationSpecification` - Cần khấu hao
- `AssetByPriceRangeSpecification` - Theo giá
- `WarrantyExpiringSoonSpecification` - Bảo hành sắp hết

**Extended Specifications:**

- `MaintenanceDueSpecification` - Cần bảo trì
- `MaintenanceOverdueSpecification` - Bảo trì quá hạn
- `WarrantyExpiredSpecification` - Hết bảo hành
- `LicenseExpiringSoonSpecification` - License sắp hết
- `HighRiskAssetSpecification` - High risk
- `AuditDueSpecification` - Cần kiểm toán
- `ReplacementDueSpecification` - Cần thay thế
- `InsuredAssetSpecification` - Có bảo hiểm
- `NetworkAssetSpecification` - IT assets
- `ProjectAssetSpecification` - Theo dự án
- `HighSecurityAssetSpecification` - Bảo mật cao
- `NonCompliantAssetSpecification` - Không tuân thủ
- `FullyDepreciatedSpecification` - Đã khấu hao hết
- `CostCenterSpecification` - Theo cost center

**Combinable:**

```csharp
var spec = new ActiveAssetSpecification()
    .And(new HighRiskAssetSpecification())
    .And(new MaintenanceDueSpecification());
```

### 6. Repository Pattern

**IRepository<T>** - Generic repository

```csharp
Task<T?> GetByIdAsync(Guid id);
Task<IEnumerable<T>> GetAllAsync();
Task<IEnumerable<T>> FindAsync(ISpecification<T> specification);
Task AddAsync(T entity);
void Update(T entity);
void Delete(T entity);
```

**IAssetRepository** - 50+ specialized methods

```csharp
// Basic queries
Task<Asset?> GetByAssetTagAsync(string assetTag);
Task<Asset?> GetBySerialNoAsync(string serialNo);
Task<Asset?> GetByBarcodeAsync(string barcode);

// Financial
Task<IEnumerable<Asset>> GetAssetsNeedingDepreciationAsync();
Task<IEnumerable<Asset>> GetFullyDepreciatedAsync();
Task<decimal> GetTotalAssetValueAsync(int? companyId = null);
Task<decimal> GetTotalBookValueAsync(int? companyId = null);

// Maintenance
Task<IEnumerable<Asset>> GetMaintenanceDueAsync();
Task<IEnumerable<Asset>> GetMaintenanceOverdueAsync();
Task<IEnumerable<Asset>> GetByServiceLevelAsync(string serviceLevel);

// Expiration
Task<IEnumerable<Asset>> GetWarrantyExpiringSoonAsync(int daysThreshold = 30);
Task<IEnumerable<Asset>> GetWarrantyExpiredAsync();
Task<IEnumerable<Asset>> GetLicenseExpiringSoonAsync(int daysThreshold = 30);
Task<IEnumerable<Asset>> GetInsuranceExpiringSoonAsync(int daysThreshold = 30);

// Risk & Compliance
Task<IEnumerable<Asset>> GetHighRiskAssetsAsync();
Task<IEnumerable<Asset>> GetCriticalAssetsAsync();
Task<IEnumerable<Asset>> GetAuditDueAsync();
Task<IEnumerable<Asset>> GetNonCompliantAsync();

// IT Assets
Task<IEnumerable<Asset>> GetNetworkAssetsAsync();
Task<Asset?> GetByIPAddressAsync(string ipAddress);
Task<bool> IsIPAddressUniqueAsync(string ipAddress, Guid? excludeAssetId = null);

// Statistics
Task<Dictionary<string, int>> GetAssetCountByTypeAsync();
Task<Dictionary<string, decimal>> GetAssetValueByDepartmentAsync();
```

### 7. Domain Events (13 Events)

Events cho business logic triggers:

- `AssetCreated` - Asset mới được tạo
- `AssetUpdated` - Asset được cập nhật
- `AssetDeleted` - Hard delete
- `AssetSoftDeleted` - Soft delete
- `AssetRestored` - Khôi phục từ soft delete
- `AssetAssigned` - Bàn giao cho user
- `AssetReleased` - Thu hồi từ user
- `AssetMaintenancePerformed` - Bảo trì hoàn thành
- `AssetExpirationWarning` - Cảnh báo hết hạn (warranty, license, insurance)
- `AssetReplacementRequired` - Cần thay thế
- `AssetDepreciationCalculated` - Khấu hao calculated
- `AssetAudited` - Kiểm toán hoàn thành
- `AssetConditionChanged` - Tình trạng thay đổi

## 🔧 Business Methods trong Asset

### Asset Creation & Update

```csharp
public static Asset Create(
    string name, 
    int companyId, 
    string assetTag,
    string createdBy)

public void UpdateBasicInfo(
    string name,
    string? description,
    string updatedBy)
```

### Identification

```csharp
public void SetIdentification(
    string? barcode,
    string? qrCode,
    string? rfidTag,
    string updatedBy)
```

### Financial Management

```csharp
public void SetFinanceInfo(
    decimal currentBookValue,
    decimal accumulatedDepreciation,
    string? accountingCode,
    string? costCenter,
    string? glAccount,
    string updatedBy)

public void UpdateDepreciation(
    decimal newBookValue,
    decimal additionalDepreciation,
    string updatedBy)

public bool IsFullyDepreciated()
```

### Insurance

```csharp
public void SetInsurance(
    string policyNo,
    decimal insuredValue,
    DateTime? expiryDate,
    string? provider,
    string updatedBy)

public bool IsInsuranceExpired()
public bool IsInsuranceExpiringSoon(int daysThreshold = 30)
public void SetRiskLevel(string riskLevel, string updatedBy)
public bool IsCriticalAsset()
```

### Maintenance

```csharp
public void ScheduleMaintenance(
    DateTime nextMaintenanceDate,
    int intervalDays,
    string? contractNo,
    string updatedBy)

public void RecordMaintenance(
    string performedBy,
    string? notes,
    int? nextIntervalDays)

public bool IsMaintenanceDue()
public bool IsMaintenanceOverdue()
public int? DaysUntilMaintenance()
```

### IT Asset Management

```csharp
public void SetITInfo(
    string? ipAddress,
    string? macAddress,
    string? hostname,
    string updatedBy)

public void SetSoftwareInfo(
    string? os,
    string? version,
    string? licenseKey,
    DateTime? licenseExpiry,
    int? licenseCount,
    string updatedBy)

public bool IsLicenseExpired()
public bool IsLicenseExpiringSoon(int daysThreshold = 30)
```

### Compliance & Security

```csharp
public void SetCompliance(
    string complianceStatus,
    string? dataClassification,
    string updatedBy)

public void SetSecurity(
    string securityClassification,
    bool requiresBackgroundCheck,
    string updatedBy)

public void ScheduleAudit(
    DateTime nextAuditDate,
    string updatedBy)

public bool IsAuditDue()
public bool IsHighSecurity()
```

### Health Status

```csharp
public AssetHealthStatus GetHealthStatus()
// Returns: Healthy, NeedsAttention, Critical
// Based on: maintenance overdue, warranty expired, 
//           license expired, insurance expired, audit overdue
```

## 📊 Enum Types

### AssetHealthStatus

- `Healthy` - Tất cả OK
- `NeedsAttention` - Cần chú ý (warranty/license sắp hết, bảo trì sắp đến)
- `Critical` - Nghiêm trọng (quá hạn bảo trì, hết bảo hiểm, etc.)

### RiskLevel

- `Low`
- `Medium`
- `High`
- `Critical`

### AssetHealthStatus (Lifecycle Manager)

- `Excellent`
- `Good`
- `Fair`
- `Poor`
- `Critical`

### AssetActionType

- `None`
- `Maintenance`
- `Repair`
- `Upgrade`
- `Replace`
- `Dispose`
- `RenewWarranty`
- `RenewInsurance`
- `RenewLicense`
- `Audit`

### ComplianceSeverity

- `Low`
- `Medium`
- `High`
- `Critical`

## 🏗️ Architecture Patterns

### Aggregate Pattern

- **Asset Aggregate**: Asset (root) → Assignment, AssetEvent, Attachment
- **Location Aggregate**: Location (root) → Building, Floor, Room

### Repository Pattern

- Generic repository với base CRUD operations
- Specialized repositories với domain-specific queries
- Unit of Work cho transaction management

### Specification Pattern

- Reusable query logic
- Combinable với And, Or, Not
- Type-safe querying

### Domain Events

- Decouple business logic
- Enable side effects
- Support event sourcing

## 📝 Best Practices

1. **Encapsulation**: Tất cả properties là read-only từ bên ngoài
2. **Validation**: Business rules trong domain methods
3. **Immutability**: Value Objects là immutable
4. **Factory Methods**: Sử dụng static Create methods
5. **Domain Events**: Raise events cho state changes
6. **Specifications**: Query logic tái sử dụng được
7. **No Infrastructure**: Pure domain logic, no dependencies

## 🔄 Workflow Examples

### Tạo Asset Mới

```csharp
var asset = Asset.Create("Laptop Dell XPS 15", companyId, "ASSET-001", "admin");
asset.SetIdentification("BARCODE123", "QR123", null, "admin");
asset.SetFinanceInfo(50000, 0, "ACC-001", "CC-IT", "GL-ASSET", "admin");
asset.SetInsurance("INS-001", 55000, DateTime.Now.AddYears(1), "Provider", "admin");
asset.ScheduleMaintenance(DateTime.Now.AddMonths(6), 180, "MAINT-001", "admin");

await assetRepository.AddAsync(asset);
await unitOfWork.SaveChangesAsync();
```

### Kiểm Tra Health Status

```csharp
var asset = await assetRepository.GetByAssetTagAsync("ASSET-001");
var health = asset.GetHealthStatus();

if (health == AssetHealthStatus.Critical)
{
    // Send alert
}
```

### Query với Specifications

```csharp
var criticalAssets = new HighRiskAssetSpecification()
    .And(new MaintenanceOverdueSpecification())
    .And(new ActiveAssetSpecification());

var assets = await assetRepository.FindAsync(criticalAssets);
```

## 📈 Statistics & Reporting

Repository hỗ trợ các queries thống kê:

```csharp
// Tổng giá trị tài sản
var totalValue = await assetRepository.GetTotalAssetValueAsync();
var totalBookValue = await assetRepository.GetTotalBookValueAsync();

// Số lượng theo loại
var countByType = await assetRepository.GetAssetCountByTypeAsync();

// Giá trị theo phòng ban
var valueByDept = await assetRepository.GetAssetValueByDepartmentAsync();

// Assets cần attention
var maintenanceDue = await assetRepository.GetMaintenanceDueAsync();
var warrantyExpiring = await assetRepository.GetWarrantyExpiringSoonAsync(30);
var highRisk = await assetRepository.GetHighRiskAssetsAsync();
```

## 🚀 Next Steps

1. **Infrastructure Layer**: Implement repositories với EF Core
2. **Application Layer**: CQRS commands/queries
3. **API Layer**: Expose endpoints
4. **Database Migration**: Create tables cho new fields
5. **Unit Tests**: Test domain logic
6. **Integration Tests**: Test với database

## 📚 Documentation

- [AGGREGATES.md](./AGGREGATES.md) - Chi tiết về Aggregates
- [IMPLEMENTATION-SUMMARY.md](./IMPLEMENTATION-SUMMARY.md) - Tổng kết implementation
- [ARCHITECTURE-DIAGRAM.txt](./ARCHITECTURE-DIAGRAM.txt) - Architecture diagram
- [db-soft-delete-migration.sql](./db-soft-delete-migration.sql) - Database migration

## ✅ Build Status

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Total Files**: 60+ C# files
**Lines of Code**: ~10,000+ lines (estimated)
**Patterns**: DDD, Repository, Specification, Domain Events, Value Objects, Aggregates

---

**Version**: 1.0.0  
**Last Updated**: 2024  
**Target Framework**: .NET 8.0

## Cấu trúc

### Common/

Base classes và interfaces cho toàn bộ domain:

- `Entity.cs` - Base entity với Id và soft delete
- `AggregateRoot.cs` - Aggregate root có thể chứa domain events
- `AuditableEntity.cs` - Entity có audit fields (CreatedAt, UpdatedAt, v.v.)
- `ValueObject.cs` - Base class cho value objects
- `IHasDomainEvents.cs` - Interface cho entities có domain events
- `IDomainEvent.cs` - Interface cho domain events

### ValueObjects/

Immutable value objects:

- `Money.cs` - Tiền tệ (Amount + Currency)
- `Address.cs` - Địa chỉ đầy đủ
- `DateRange.cs` - Khoảng thời gian
- `Email.cs` - Email với validation
- `PhoneNumber.cs` - Số điện thoại
- `DepreciationInfo.cs` - Thông tin khấu hao

### Abstractions/

Repository interfaces và patterns:

- `IRepository<T>` - Generic repository
- `IAssetRepository.cs` - Repository cho Asset aggregate
- `ILocationRepository.cs` - Repository cho Location aggregate
- `IUnitOfWork.cs` - Unit of Work pattern
- `ISpecification.cs` - Specification pattern với combinators

### Services/

Domain services (business logic không thuộc về entity):

- `IDepreciationService.cs` - Tính toán khấu hao
- `IAssetTagGenerator.cs` - Generate mã tài sản
- `ILocationHierarchyService.cs` - Quản lý cây phân cấp
- `IAssetLifecycleValidator.cs` - Validate lifecycle transitions

### Assets/ (Aggregate Root)

Domain chính về tài sản:

- `Asset.cs` - Tài sản (Aggregate Root)
- `AssetId.cs` - Value object cho Asset ID
- `Assignment.cs` - Bàn giao/thu hồi tài sản
- `AssetEvent.cs` - Sự kiện tài sản (audit log)
- `Attachment.cs` - File đính kèm
- `Enums/AssetStatus.cs` - Enums cho lifecycle, usage status, depreciation method
- `Events/` - Domain events (AssetCreated, AssetUpdated, AssetDeleted, AssetAssigned, AssetReleased, AssetSoftDeleted,
  AssetRestored)
- `Specifications/` - Business rules specifications

### Types/

Loại tài sản (OA, IT, SW, CA, WA, PU, MT):

- `AssetType.cs`
- `Events/AssetTypeCreated.cs`

### Categories/

Danh mục tài sản:

- `AssetCategory.cs`
- `Events/AssetCategoryCreated.cs`

### Geography/

Địa lý (quốc gia):

- `Country.cs` - ISO2 code
- `Events/CountryCreated.cs`

### Locations/ (Aggregate Root)

Địa điểm (hierarchical tree):

- `Location.cs` - Hỗ trợ phân cấp với Parent/Children (Aggregate Root)
- `Events/LocationCreated.cs`

### Companies/

Công ty:

- `Company.cs`

### Departments/

Phòng ban:

- `Department.cs`

### Users/

Người dùng:

- `User.cs`

### Manufacturers/

Nhà sản xuất:

- `Manufacturer.cs`

### Models/

Model/kiểu máy:

- `Model.cs`

### Suppliers/

Nhà cung cấp:

- `Supplier.cs`

### Conditions/

Tình trạng tài sản:

- `AssetCondition.cs`

### Statuses/

Các loại trạng thái:

- `LifecycleStatus.cs` - Trạng thái vòng đời (draft, pending_approval, active, v.v.)
- `UsageStatus.cs` - Trạng thái sử dụng (available, in_use, under_repair)
- `AssetEventType.cs` - Loại sự kiện (created, assigned, released, v.v.)

### Finance/

Tài chính:

- `FinanceEntry.cs` - Bút toán tài chính (khấu hao, điều chỉnh, xóa sổ)

## Nguyên tắc DDD

1. **Pure Domain** - Không có dependencies vào Infrastructure, Application, hoặc external libraries
2. **Rich Domain Model** - Entities có business logic, không chỉ là data containers
3. **Encapsulation** - Private setters, factory methods, business methods
4. **Domain Events** - Aggregate roots raise domain events cho side effects
5. **Value Objects** - Immutable objects cho domain concepts (Money, Email, AssetId, v.v.)
6. **Soft Delete** - Tất cả entities có IsDeleted, DeletedAt, DeletedBy để tránh xóa hẳn data
7. **Aggregates** - Clear boundaries (Asset, Location là aggregate roots)
8. **Repository Pattern** - One repository per aggregate root
9. **Specification Pattern** - Encapsulate query logic, reusable, combinable
10. **Domain Services** - Business logic spanning multiple entities

## Tactical Patterns Implemented

### 1. Entities & Value Objects

- **Entities**: Có identity (Id), mutable
- **Value Objects**: Không có identity, immutable, equality by value
- **Aggregate Roots**: Entities có thể chứa domain events

### 2. Repository Pattern

```csharp
// Repository interface trong Domain
public interface IAssetRepository : IRepository<Asset>
{
    Task<Asset?> GetByAssetTagAsync(string assetTag);
    // ...domain-specific queries
}

// Implementation trong Infrastructure
```

### 3. Specification Pattern

```csharp
var spec = new ActiveAssetSpecification()
    .And(new AssetByCompanySpecification(companyId))
    .And(new AvailableAssetSpecification());

var assets = await repository.FindAsync(spec.ToExpression());
```

### 4. Domain Services

```csharp
// Business logic không thuộc về entity nào
var bookValue = depreciationService.CalculateCurrentBookValue(asset, DateTime.Now);
var isValid = lifecycleValidator.CanTransitionTo(asset, "active");
```

### 5. Domain Events

```csharp
asset.Assign("user", userId, currentUserId);
// Raises AssetAssigned domain event
// Event handlers trong Application layer
```

### 6. Unit of Work

```csharp
await unitOfWork.BeginTransactionAsync();
try
{
    var asset = await unitOfWork.Assets.GetByIdAsync(id);
    asset.UpdateBasicInfo(name, notes, userId);
    await unitOfWork.SaveChangesAsync();
    await unitOfWork.CommitTransactionAsync();
}
catch
{
    await unitOfWork.RollbackTransactionAsync();
}
```

## Aggregates

Xem chi tiết trong `AGGREGATES.md`:

- **Asset Aggregate**: Asset (root) + Assignment + AssetEvent + Attachment
- **Location Aggregate**: Location (root) + Children

## Soft Delete Pattern

Tất cả entities kế thừa từ `Entity` base class đều có:

- `IsDeleted` (bool) - Đánh dấu đã xóa
- `DeletedAt` (DateTime?) - Thời điểm xóa
- `DeletedBy` (int?) - User ID người xóa

Methods:

- `SoftDelete(int? deletedBy)` - Xóa mềm
- `Restore()` - Khôi phục

**Lợi ích:**

- Không mất dữ liệu vĩnh viễn
- Có thể audit/track lịch sử xóa
- Dễ dàng khôi phục khi cần
- Compliance với quy định lưu trữ dữ liệu

## Notes

- Tất cả entities kế thừa từ `Entity` hoặc `AggregateRoot`
- Sử dụng private constructors + static factory methods
- Navigation properties để public cho EF Core nhưng nên dùng Repository pattern
- Domain events được raise trong aggregate roots và handle bởi Application layer
