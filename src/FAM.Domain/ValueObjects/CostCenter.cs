using FAM.Domain.Common;

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
            throw new DomainException("Cost center cannot be empty");

        costCenter = costCenter.Trim();

        if (costCenter.Length > 30)
            throw new DomainException("Cost center cannot exceed 30 characters");

        return new CostCenter(costCenter);
    }

    /// <summary>
    /// Implicit conversion từ CostCenter sang string
    /// </summary>
    public static implicit operator string(CostCenter costCenter) => costCenter.Value;

    /// <summary>
    /// Explicit conversion từ string sang CostCenter
    /// </summary>
    public static explicit operator CostCenter(string costCenter) => Create(costCenter);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}