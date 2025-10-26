using FAM.Domain.Abstractions;

namespace FAM.Domain.Assets.Specifications;

/// <summary>
/// Specification: Tài sản cần bảo trì
/// </summary>
public class MaintenanceDueSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        var today = DateTime.UtcNow.Date;
        return asset =>
            !asset.IsDeleted &&
            asset.NextMaintenanceDate.HasValue &&
            asset.NextMaintenanceDate.Value <= today;
    }
}

/// <summary>
/// Specification: Tài sản bảo trì quá hạn
/// </summary>
public class MaintenanceOverdueSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        var today = DateTime.UtcNow.Date;
        return asset =>
            !asset.IsDeleted &&
            asset.NextMaintenanceDate.HasValue &&
            asset.NextMaintenanceDate.Value < today;
    }
}

/// <summary>
/// Specification: Bảo hành đã hết hạn
/// </summary>
public class WarrantyExpiredSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        var today = DateTime.UtcNow;
        return asset =>
            !asset.IsDeleted &&
            asset.WarrantyUntil.HasValue &&
            asset.WarrantyUntil.Value < today;
    }
}

/// <summary>
/// Specification: License sắp hết hạn
/// </summary>
public class LicenseExpiringSoonSpecification : Specification<Asset>
{
    private readonly int _daysThreshold;

    public LicenseExpiringSoonSpecification(int daysThreshold = 30)
    {
        _daysThreshold = daysThreshold;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        var checkDate = DateTime.UtcNow.AddDays(_daysThreshold);
        var today = DateTime.UtcNow;
        
        return asset =>
            !asset.IsDeleted &&
            asset.LicenseExpiryDate.HasValue &&
            asset.LicenseExpiryDate.Value <= checkDate &&
            asset.LicenseExpiryDate.Value > today;
    }
}

/// <summary>
/// Specification: Tài sản có mức độ rủi ro cao
/// </summary>
public class HighRiskAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            (asset.RiskLevel == "High" || asset.RiskLevel == "Critical");
    }
}

/// <summary>
/// Specification: Tài sản tới hạn kiểm toán
/// </summary>
public class AuditDueSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        var today = DateTime.UtcNow.Date;
        return asset =>
            !asset.IsDeleted &&
            asset.NextAuditDate.HasValue &&
            asset.NextAuditDate.Value <= today;
    }
}

/// <summary>
/// Specification: Tài sản cần thay thế
/// </summary>
public class ReplacementDueSpecification : Specification<Asset>
{
    private readonly int _monthsThreshold;

    public ReplacementDueSpecification(int monthsThreshold = 0)
    {
        _monthsThreshold = monthsThreshold;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.EstimatedRemainingLifeMonths.HasValue &&
            asset.EstimatedRemainingLifeMonths.Value <= _monthsThreshold;
    }
}

/// <summary>
/// Specification: Tài sản có bảo hiểm
/// </summary>
public class InsuredAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            !string.IsNullOrEmpty(asset.InsurancePolicyNo) &&
            asset.InsuranceExpiryDate.HasValue &&
            asset.InsuranceExpiryDate.Value > DateTime.UtcNow;
    }
}

/// <summary>
/// Specification: Tài sản IT có IP address
/// </summary>
public class NetworkAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.IPAddress != null;
    }
}

/// <summary>
/// Specification: Tài sản thuộc dự án cụ thể
/// </summary>
public class ProjectAssetSpecification : Specification<Asset>
{
    private readonly string _projectCode;

    public ProjectAssetSpecification(string projectCode)
    {
        _projectCode = projectCode;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.ProjectCode == _projectCode;
    }
}

/// <summary>
/// Specification: Tài sản có phân loại bảo mật cao
/// </summary>
public class HighSecurityAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            (asset.SecurityClassification == "Confidential" || 
             asset.SecurityClassification == "Secret");
    }
}

/// <summary>
/// Specification: Tài sản không tuân thủ quy định
/// </summary>
public class NonCompliantAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.ComplianceStatus == "NonCompliant";
    }
}

/// <summary>
/// Specification: Tài sản đã khấu hao hết
/// </summary>
public class FullyDepreciatedSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.CurrentBookValue.HasValue &&
            asset.ResidualValue.HasValue &&
            asset.CurrentBookValue.Value <= asset.ResidualValue.Value;
    }
}

/// <summary>
/// Specification: Tài sản có cost center cụ thể
/// </summary>
public class CostCenterSpecification : Specification<Asset>
{
    private readonly string _costCenter;

    public CostCenterSpecification(string costCenter)
    {
        _costCenter = costCenter;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.CostCenter == _costCenter;
    }
}
