using FAM.Domain.Abstractions;

namespace FAM.Domain.Assets.Specifications;

/// <summary>
/// Specification: Tài sản chưa bị xóa
/// </summary>
public class ActiveAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset => !asset.IsDeleted;
    }
}

/// <summary>
/// Specification: Tài sản theo công ty
/// </summary>
public class AssetByCompanySpecification : Specification<Asset>
{
    private readonly int _companyId;

    public AssetByCompanySpecification(int companyId)
    {
        _companyId = companyId;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset => asset.CompanyId == _companyId && !asset.IsDeleted;
    }
}

/// <summary>
/// Specification: Tài sản đang hoạt động
/// </summary>
public class ActiveLifecycleAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset => asset.LifecycleCode == "active" && !asset.IsDeleted;
    }
}

/// <summary>
/// Specification: Tài sản sẵn sàng để bàn giao
/// </summary>
public class AvailableAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            asset.UsageCode == "available" &&
            asset.LifecycleCode == "active" &&
            !asset.IsDeleted;
    }
}

/// <summary>
/// Specification: Tài sản đang được sử dụng
/// </summary>
public class InUseAssetSpecification : Specification<Asset>
{
    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset => asset.UsageCode == "in_use" && !asset.IsDeleted;
    }
}

/// <summary>
/// Specification: Tài sản cần khấu hao trong kỳ
/// </summary>
public class AssetNeedingDepreciationSpecification : Specification<Asset>
{
    private readonly DateTime _period;

    public AssetNeedingDepreciationSpecification(DateTime period)
    {
        _period = period;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.InServiceDate.HasValue &&
            asset.InServiceDate.Value <= _period &&
            asset.UsefulLifeMonths.HasValue &&
            asset.DepreciationMethod != null;
    }
}

/// <summary>
/// Specification: Tài sản theo khoảng giá mua
/// </summary>
public class AssetByPriceRangeSpecification : Specification<Asset>
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public AssetByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        return asset =>
            !asset.IsDeleted &&
            asset.PurchaseCost.HasValue &&
            asset.PurchaseCost >= _minPrice &&
            asset.PurchaseCost <= _maxPrice;
    }
}

/// <summary>
/// Specification: Tài sản sắp hết bảo hành
/// </summary>
public class WarrantyExpiringSoonSpecification : Specification<Asset>
{
    private readonly int _daysBeforeExpiry;

    public WarrantyExpiringSoonSpecification(int daysBeforeExpiry = 30)
    {
        _daysBeforeExpiry = daysBeforeExpiry;
    }

    public override System.Linq.Expressions.Expression<Func<Asset, bool>> ToExpression()
    {
        var checkDate = DateTime.UtcNow.AddDays(_daysBeforeExpiry);
        return asset =>
            !asset.IsDeleted &&
            asset.WarrantyUntil.HasValue &&
            asset.WarrantyUntil.Value <= checkDate &&
            asset.WarrantyUntil.Value >= DateTime.UtcNow;
    }
}