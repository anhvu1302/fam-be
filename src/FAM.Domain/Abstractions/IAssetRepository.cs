using FAM.Domain.Assets;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository cho Asset aggregate
/// </summary>
public interface IAssetRepository : IRepository<Asset>
{
    // Basic queries
    Task<Asset?> GetByAssetTagAsync(string assetTag, CancellationToken cancellationToken = default);
    Task<Asset?> GetBySerialNoAsync(string serialNo, CancellationToken cancellationToken = default);
    Task<Asset?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByLocationIdAsync(int locationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default);
    
    // Status queries
    Task<IEnumerable<Asset>> GetByLifecycleStatusAsync(string lifecycleCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByUsageStatusAsync(string usageCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetAvailableAssetsAsync(CancellationToken cancellationToken = default);
    
    // Financial queries
    Task<IEnumerable<Asset>> GetAssetsNeedingDepreciationAsync(DateTime period, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetFullyDepreciatedAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByCostCenterAsync(string costCenter, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAssetValueAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalBookValueAsync(int? companyId = null, CancellationToken cancellationToken = default);
    
    // Maintenance & Support queries
    Task<IEnumerable<Asset>> GetMaintenanceDueAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetMaintenanceOverdueAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetAssetsByServiceLevelAsync(string serviceLevel, CancellationToken cancellationToken = default);
    
    // Expiration queries
    Task<IEnumerable<Asset>> GetWarrantyExpiringSoonAsync(int daysThreshold = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetWarrantyExpiredAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetLicenseExpiringSoonAsync(int daysThreshold = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetLicenseExpiredAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetInsuranceExpiringSoonAsync(int daysThreshold = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetSupportExpiredAssetsAsync(CancellationToken cancellationToken = default);
    
    // Risk & Compliance queries
    Task<IEnumerable<Asset>> GetHighRiskAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetCriticalAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetAuditDueAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetNonCompliantAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetHighSecurityAssetsAsync(CancellationToken cancellationToken = default);
    
    // Replacement queries
    Task<IEnumerable<Asset>> GetReplacementDueAssetsAsync(int monthsThreshold = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetEndOfLifeAssetsAsync(int daysThreshold = 90, CancellationToken cancellationToken = default);
    
    // Project & Campaign queries
    Task<IEnumerable<Asset>> GetByProjectCodeAsync(string projectCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByCampaignCodeAsync(string campaignCode, CancellationToken cancellationToken = default);
    
    // IT specific queries
    Task<Asset?> GetByIPAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<Asset?> GetByMACAddressAsync(string macAddress, CancellationToken cancellationToken = default);
    Task<Asset?> GetByHostnameAsync(string hostname, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetNetworkAssetsAsync(CancellationToken cancellationToken = default);
    
    // Validation queries
    Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeAssetId = null, CancellationToken cancellationToken = default);
    Task<bool> IsSerialNoUniqueAsync(string serialNo, int? excludeAssetId = null, CancellationToken cancellationToken = default);
    Task<bool> IsIPAddressUniqueAsync(string ipAddress, int? excludeAssetId = null, CancellationToken cancellationToken = default);
    
    // Statistics & Reporting
    Task<Dictionary<string, int>> GetAssetCountByTypeAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetAssetCountByLocationAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetAssetCountByStatusAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetAssetValueByDepartmentAsync(int? companyId = null, CancellationToken cancellationToken = default);
}
