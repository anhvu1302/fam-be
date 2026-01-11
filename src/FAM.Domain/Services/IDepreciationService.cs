using FAM.Domain.Assets;

namespace FAM.Domain.Services;

/// <summary>
/// Domain Service: Tính toán khấu hao tài sản
/// </summary>
public interface IDepreciationService
{
    decimal CalculateMonthlyDepreciation(Asset asset);
    decimal CalculateCurrentBookValue(Asset asset, DateTime asOfDate);
    decimal CalculateTotalDepreciation(Asset asset, DateTime asOfDate);
    int GetElapsedMonths(Asset asset, DateTime asOfDate);
    bool IsFullyDepreciated(Asset asset, DateTime asOfDate);
}

public class DepreciationService : IDepreciationService
{
    public decimal CalculateMonthlyDepreciation(Asset asset)
    {
        if (!asset.PurchaseCost.HasValue || !asset.UsefulLifeMonths.HasValue)
        {
            return 0;
        }

        if (string.IsNullOrEmpty(asset.DepreciationMethod))
        {
            return 0;
        }

        decimal residualValue = asset.ResidualValue ?? 0;
        decimal depreciableAmount = asset.PurchaseCost.Value - residualValue;

        return asset.DepreciationMethod.ToLowerInvariant() switch
        {
            "straightline" => depreciableAmount / asset.UsefulLifeMonths.Value,
            "decliningbalance" => CalculateDecliningBalanceDepreciation(asset),
            _ => 0
        };
    }

    public decimal CalculateCurrentBookValue(Asset asset, DateTime asOfDate)
    {
        if (!asset.PurchaseCost.HasValue)
        {
            return 0;
        }

        decimal totalDepreciation = CalculateTotalDepreciation(asset, asOfDate);
        decimal bookValue = asset.PurchaseCost.Value - totalDepreciation;
        decimal residualValue = asset.ResidualValue ?? 0;

        return Math.Max(bookValue, residualValue);
    }

    public decimal CalculateTotalDepreciation(Asset asset, DateTime asOfDate)
    {
        if (!asset.InServiceDate.HasValue)
        {
            return 0;
        }

        if (asOfDate < asset.InServiceDate.Value)
        {
            return 0;
        }

        int elapsedMonths = GetElapsedMonths(asset, asOfDate);
        decimal monthlyDepreciation = CalculateMonthlyDepreciation(asset);

        int usefulLife = asset.UsefulLifeMonths ?? 0;
        int monthsToDepreciate = Math.Min(elapsedMonths, usefulLife);

        return monthlyDepreciation * monthsToDepreciate;
    }

    public int GetElapsedMonths(Asset asset, DateTime asOfDate)
    {
        if (!asset.InServiceDate.HasValue)
        {
            return 0;
        }

        int months = 0;
        DateTime current = asset.InServiceDate.Value;

        while (current < asOfDate)
        {
            months++;
            current = current.AddMonths(1);
        }

        return months;
    }

    public bool IsFullyDepreciated(Asset asset, DateTime asOfDate)
    {
        if (!asset.UsefulLifeMonths.HasValue || !asset.InServiceDate.HasValue)
        {
            return false;
        }

        int elapsedMonths = GetElapsedMonths(asset, asOfDate);
        return elapsedMonths >= asset.UsefulLifeMonths.Value;
    }

    private decimal CalculateDecliningBalanceDepreciation(Asset asset)
    {
        // Declining balance: 200% / useful life (double declining)
        if (!asset.PurchaseCost.HasValue || !asset.UsefulLifeMonths.HasValue)
        {
            return 0;
        }

        decimal rate = 2.0m / asset.UsefulLifeMonths.Value;
        return asset.PurchaseCost.Value * rate;
    }
}
