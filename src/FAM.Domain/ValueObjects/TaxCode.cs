using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Tax Code Value Object - đảm bảo tính toàn vẹn của tax code
/// </summary>
public sealed class TaxCode : ValueObject
{
    public string Value { get; private set; }

    private TaxCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo TaxCode từ string với validation
    /// </summary>
    public static TaxCode Create(string taxCode)
    {
        if (string.IsNullOrWhiteSpace(taxCode))
            throw new DomainException("Tax code cannot be empty");

        taxCode = taxCode.Trim().ToUpperInvariant();

        if (taxCode.Length < 8 || taxCode.Length > 15)
            throw new DomainException("Tax code must be between 8 and 15 characters");

        // Basic validation for Vietnamese tax code format (simplified)
        if (!System.Text.RegularExpressions.Regex.IsMatch(taxCode, @"^[0-9\-]+$"))
            throw new DomainException("Tax code can only contain numbers and hyphens");

        return new TaxCode(taxCode);
    }

    /// <summary>
    /// Implicit conversion từ TaxCode sang string
    /// </summary>
    public static implicit operator string(TaxCode taxCode) => taxCode.Value;

    /// <summary>
    /// Explicit conversion từ string sang TaxCode
    /// </summary>
    public static explicit operator TaxCode(string taxCode) => Create(taxCode);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}