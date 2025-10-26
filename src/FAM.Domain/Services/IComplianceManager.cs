using FAM.Domain.Assets;

namespace FAM.Domain.Services;

/// <summary>
/// Service quản lý tuân thủ và kiểm toán tài sản
/// </summary>
public interface IComplianceManager
{
    /// <summary>
    /// Kiểm tra tuân thủ của tài sản
    /// </summary>
    ComplianceCheckResult CheckCompliance(Asset asset);

    /// <summary>
    /// Lấy danh sách tài sản không tuân thủ
    /// </summary>
    Task<IEnumerable<Asset>> GetNonCompliantAssets();

    /// <summary>
    /// Lấy danh sách tài sản cần kiểm toán
    /// </summary>
    Task<IEnumerable<Asset>> GetAssetsDueForAudit(int daysThreshold = 0);

    /// <summary>
    /// Lập lịch kiểm toán cho tài sản
    /// </summary>
    void ScheduleAudit(Asset asset, DateTime auditDate);

    /// <summary>
    /// Kiểm tra yêu cầu bảo mật của tài sản
    /// </summary>
    SecurityRequirements GetSecurityRequirements(Asset asset);

    /// <summary>
    /// Xác thực phân loại dữ liệu
    /// </summary>
    bool ValidateDataClassification(string dataClassification);

    /// <summary>
    /// Tạo báo cáo tuân thủ
    /// </summary>
    ComplianceReport GenerateComplianceReport(IEnumerable<Asset> assets);
}

/// <summary>
/// Kết quả kiểm tra tuân thủ
/// </summary>
public class ComplianceCheckResult
{
    public bool IsCompliant { get; set; }
    public List<ComplianceIssue> Issues { get; set; } = new();
    public DateTime CheckDate { get; set; }
    public string CheckedBy { get; set; } = string.Empty;
}

/// <summary>
/// Vấn đề tuân thủ
/// </summary>
public class ComplianceIssue
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public enum ComplianceSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Yêu cầu bảo mật
/// </summary>
public class SecurityRequirements
{
    public string SecurityClassification { get; set; } = string.Empty;
    public bool RequiresBackgroundCheck { get; set; }
    public bool RequiresEncryption { get; set; }
    public bool RequiresPhysicalSecurity { get; set; }
    public bool RequiresAccessControl { get; set; }
    public List<string> RequiredCertifications { get; set; } = new();
}

/// <summary>
/// Báo cáo tuân thủ
/// </summary>
public class ComplianceReport
{
    public DateTime ReportDate { get; set; }
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public decimal ComplianceRate => TotalAssets > 0 
        ? (decimal)CompliantAssets / TotalAssets * 100 
        : 0;
    public List<ComplianceIssue> TopIssues { get; set; } = new();
    public Dictionary<string, int> IssuesByCategory { get; set; } = new();
}
