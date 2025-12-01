using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho thông tin khấu hao
/// </summary>
public sealed class DepreciationInfo : ValueObject
{
    public string Method { get; }
    public int UsefulLifeMonths { get; }
    public decimal ResidualValue { get; }
    public DateTime? InServiceDate { get; }

    private DepreciationInfo(
        string method,
        int usefulLifeMonths,
        decimal residualValue,
        DateTime? inServiceDate)
    {
        Method = method;
        UsefulLifeMonths = usefulLifeMonths;
        ResidualValue = residualValue;
        InServiceDate = inServiceDate;
    }

    public static DepreciationInfo Create(string method, int usefulLifeMonths, decimal residualValue)
    {
        if (string.IsNullOrWhiteSpace(method))
            throw new DomainException(ErrorCodes.VO_DEPRECIATION_METHOD_EMPTY);

        if (usefulLifeMonths <= 0)
            throw new DomainException(ErrorCodes.VO_DEPRECIATION_LIFE_INVALID);

        if (residualValue < 0)
            throw new DomainException(ErrorCodes.VO_DEPRECIATION_RESIDUAL_INVALID);

        return new DepreciationInfo(method, usefulLifeMonths, residualValue, null);
    }

    public decimal CalculateMonthlyDepreciation(decimal purchaseCost)
    {
        if (Method.Equals("StraightLine", StringComparison.OrdinalIgnoreCase))
            return (purchaseCost - ResidualValue) / UsefulLifeMonths;

        // Implement other methods as needed
        return 0;
    }

    public decimal CalculateCurrentBookValue(decimal purchaseCost, int elapsedMonths)
    {
        if (elapsedMonths <= 0)
            return purchaseCost;

        var monthlyDepreciation = CalculateMonthlyDepreciation(purchaseCost);
        var totalDepreciation = monthlyDepreciation * Math.Min(elapsedMonths, UsefulLifeMonths);

        return Math.Max(purchaseCost - totalDepreciation, ResidualValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Method;
        yield return UsefulLifeMonths;
        yield return ResidualValue;
        yield return InServiceDate;
    }

    public override string ToString()
    {
        return $"{Method}, {UsefulLifeMonths} months, Residual: {ResidualValue}";
    }
}