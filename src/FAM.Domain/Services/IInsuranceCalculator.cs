using FAM.Domain.Assets;

namespace FAM.Domain.Services;

/// <summary>
/// Service tính toán và quản lý bảo hiểm tài sản
/// </summary>
public interface IInsuranceCalculator
{
    /// <summary>
    /// Tính giá trị bảo hiểm được đề xuất dựa trên giá trị hiện tại
    /// </summary>
    decimal CalculateRecommendedInsuredValue(Asset asset);

    /// <summary>
    /// Tính phí bảo hiểm ước tính
    /// </summary>
    decimal EstimateAnnualPremium(Asset asset, decimal insuredValue);

    /// <summary>
    /// Đánh giá mức độ rủi ro tài sản
    /// </summary>
    RiskLevel AssessRiskLevel(Asset asset);

    /// <summary>
    /// Kiểm tra xem tài sản có cần bảo hiểm không
    /// </summary>
    bool RequiresInsurance(Asset asset);

    /// <summary>
    /// Lấy danh sách tài sản cần gia hạn bảo hiểm
    /// </summary>
    Task<IEnumerable<Asset>> GetAssetsRequiringInsuranceRenewal(int daysThreshold = 30);
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}
