using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Cost Center Value Object - đảm bảo tính toàn vẹn của cost center
/// </summary>
public sealed class CostCenter : ValueObject
{
    public string Value { get; private set; }

    private CostCenter(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo CostCenter từ string với validation
    /// </summary>
    public static CostCenter Create(string costCenter)
    {
        if (string.IsNullOrWhiteSpace(costCenter))
            throw new DomainException(ErrorCodes.VO_COST_CENTER_EMPTY);

        costCenter = costCenter.Trim();

        if (costCenter.Length > 30)
            throw new DomainException(ErrorCodes.VO_COST_CENTER_TOO_LONG);

        return new CostCenter(costCenter);
    }

    /// <summary>
    /// Implicit conversion từ CostCenter sang string
    /// </summary>
    public static implicit operator string(CostCenter costCenter)
    {
        return costCenter.Value;
    }

    /// <summary>
    /// Explicit conversion từ string sang CostCenter
    /// </summary>
    public static explicit operator CostCenter(string costCenter)
    {
        return Create(costCenter);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}