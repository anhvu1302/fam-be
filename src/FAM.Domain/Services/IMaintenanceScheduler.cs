using FAM.Domain.Assets;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Services;

/// <summary>
/// Service lập lịch và quản lý bảo trì tài sản
/// </summary>
public interface IMaintenanceScheduler
{
    /// <summary>
    /// Lập lịch bảo trì định kỳ cho tài sản
    /// </summary>
    void SchedulePeriodicMaintenance(
        Asset asset,
        int intervalDays,
        DateTime? firstMaintenanceDate = null);

    /// <summary>
    /// Tính toán ngày bảo trì tiếp theo dựa trên lịch sử
    /// </summary>
    DateTime CalculateNextMaintenanceDate(
        Asset asset,
        DateTime lastMaintenanceDate,
        int intervalDays);

    /// <summary>
    /// Kiểm tra xem tài sản có cần bảo trì khẩn cấp không
    /// </summary>
    bool RequiresUrgentMaintenance(Asset asset);

    /// <summary>
    /// Lấy danh sách tài sản cần bảo trì
    /// </summary>
    Task<IEnumerable<Asset>> GetAssetsDueForMaintenance();

    /// <summary>
    /// Lấy danh sách tài sản quá hạn bảo trì
    /// </summary>
    Task<IEnumerable<Asset>> GetOverdueMaintenanceAssets();

    /// <summary>
    /// Tính toán chi phí bảo trì ước tính
    /// </summary>
    decimal EstimateMaintenanceCost(Asset asset, string maintenanceType);

    /// <summary>
    /// Tạo kế hoạch bảo trì cho tài sản trong một khoảng thời gian
    /// </summary>
    MaintenancePlan GenerateMaintenancePlan(Asset asset, DateRange period);
}

/// <summary>
/// Kế hoạch bảo trì
/// </summary>
public class MaintenancePlan
{
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public List<ScheduledMaintenance> ScheduledMaintenances { get; set; } = new();
    public decimal EstimatedTotalCost { get; set; }
}

/// <summary>
/// Bảo trì đã lập lịch
/// </summary>
public class ScheduledMaintenance
{
    public DateTime ScheduledDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public bool IsUrgent { get; set; }
}
