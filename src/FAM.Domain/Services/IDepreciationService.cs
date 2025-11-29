using FAM.Domain.Assets;
using FAM.Domain.Abstractions;

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
            return 0;

        if (string.IsNullOrEmpty(asset.DepreciationMethod))
            return 0;

        var residualValue = asset.ResidualValue ?? 0;
        var depreciableAmount = asset.PurchaseCost.Value - residualValue;

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
            return 0;

        var totalDepreciation = CalculateTotalDepreciation(asset, asOfDate);
        var bookValue = asset.PurchaseCost.Value - totalDepreciation;
        var residualValue = asset.ResidualValue ?? 0;

        return Math.Max(bookValue, residualValue);
    }

    public decimal CalculateTotalDepreciation(Asset asset, DateTime asOfDate)
    {
        if (!asset.InServiceDate.HasValue)
            return 0;

        if (asOfDate < asset.InServiceDate.Value)
            return 0;

        var elapsedMonths = GetElapsedMonths(asset, asOfDate);
        var monthlyDepreciation = CalculateMonthlyDepreciation(asset);

        var usefulLife = asset.UsefulLifeMonths ?? 0;
        var monthsToDepreciate = Math.Min(elapsedMonths, usefulLife);

        return monthlyDepreciation * monthsToDepreciate;
    }

    public int GetElapsedMonths(Asset asset, DateTime asOfDate)
    {
        if (!asset.InServiceDate.HasValue)
            return 0;

        var months = 0;
        var current = asset.InServiceDate.Value;

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
            return false;

        var elapsedMonths = GetElapsedMonths(asset, asOfDate);
        return elapsedMonths >= asset.UsefulLifeMonths.Value;
    }

    private decimal CalculateDecliningBalanceDepreciation(Asset asset)
    {
        // Declining balance: 200% / useful life (double declining)
        if (!asset.PurchaseCost.HasValue || !asset.UsefulLifeMonths.HasValue)
            return 0;

        var rate = 2.0m / asset.UsefulLifeMonths.Value;
        return asset.PurchaseCost.Value * rate;
    }
}