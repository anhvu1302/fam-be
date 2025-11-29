using FAM.Domain.Assets;

namespace FAM.Domain.Services;

/// <summary>
/// Service quản lý chu kỳ sống của tài sản
/// </summary>
public interface IAssetLifecycleManager
{
    /// <summary>
    /// Tính toán tuổi thọ còn lại của tài sản (tháng)
    /// </summary>
    int CalculateRemainingLifeMonths(Asset asset);

    /// <summary>
    /// Xác định xem tài sản có đang đến cuối vòng đời không
    /// </summary>
    bool IsNearingEndOfLife(Asset asset, int monthsThreshold = 6);

    /// <summary>
    /// Tính toán ngày cuối vòng đời dự kiến
    /// </summary>
    DateTime? CalculateExpectedEndOfLife(Asset asset);

    /// <summary>
    /// Đánh giá tình trạng sức khỏe tài sản
    /// </summary>
    AssetHealthStatus EvaluateAssetHealth(Asset asset);

    /// <summary>
    /// Đề xuất tài sản cần thay thế
    /// </summary>
    Task<IEnumerable<Asset>> GetAssetsRecommendedForReplacement();

    /// <summary>
    /// Tính toán tổng chi phí sở hữu (TCO)
    /// </summary>
    decimal CalculateTotalCostOfOwnership(Asset asset);

    /// <summary>
    /// Đề xuất hành động tiếp theo cho tài sản
    /// </summary>
    AssetAction RecommendNextAction(Asset asset);
}

public enum AssetHealthStatus
{
    Excellent,
    Good,
    Fair,
    Poor,
    Critical
}

public class AssetAction
{
    public AssetActionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime RecommendedDate { get; set; }
    public int Priority { get; set; } // 1 = High, 5 = Low
    public decimal EstimatedCost { get; set; }
}

public enum AssetActionType
{
    None,
    Maintenance,
    Repair,
    Upgrade,
    Replace,
    Dispose,
    RenewWarranty,
    RenewInsurance,
    RenewLicense,
    Audit
}