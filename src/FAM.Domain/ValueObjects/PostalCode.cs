using System.Text.RegularExpressions;

using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Postal Code Value Object - đảm bảo tính toàn vẹn của postal code
/// </summary>
public sealed class PostalCode : ValueObject
{
    public string Value { get; private set; }

    // Constructor for EF Core
    private PostalCode()
    {
        Value = string.Empty;
    }

    private PostalCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo PostalCode từ string với validation
    /// </summary>
    public static PostalCode Create(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            throw new DomainException(ErrorCodes.VO_POSTAL_CODE_EMPTY);
        }

        postalCode = postalCode.Trim().ToUpperInvariant();

        if (!IsValidPostalCode(postalCode))
        {
            throw new DomainException(ErrorCodes.VO_POSTAL_CODE_INVALID);
        }

        return new PostalCode(postalCode);
    }

    /// <summary>
    /// Kiểm tra format postal code hợp lệ
    /// </summary>
    private static bool IsValidPostalCode(string postalCode)
    {
        // Postal code patterns cho các quốc gia khác nhau
        string[] patterns = new[]
        {
            @"^\d{5}$", // US: 12345
            @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$", // Canada: K1A 1A1
            @"^\d{4}$", // Vietnam: 1234
            @"^[A-Z]{1,2}\d{1,2}[A-Z]?\s?\d[A-Z]{2}$", // UK: SW1A 1AA
            @"^\d{5}$", // Germany: 12345
            @"^\d{5}$", // France: 12345
            @"^\d{4}\s?[A-Z]{2}$", // Netherlands: 1234 AB
            @"^\d{3}-\d{4}$", // Japan: 123-4567
            @"^\d{6}$", // India: 123456
            @"^\d{4,6}$" // General numeric
        };

        return patterns.Any(pattern => Regex.IsMatch(postalCode, pattern));
    }

    /// <summary>
    /// Kiểm tra có phải postal code Việt Nam không
    /// </summary>
    public bool IsVietnamesePostalCode()
    {
        return Regex.IsMatch(Value, @"^\d{4,6}$");
    }

    /// <summary>
    /// Kiểm tra có phải postal code US không
    /// </summary>
    public bool IsUSPostalCode()
    {
        return Regex.IsMatch(Value, @"^\d{5}$");
    }

    /// <summary>
    /// Implicit conversion từ PostalCode sang string
    /// </summary>
    public static implicit operator string(PostalCode postalCode)
    {
        return postalCode.Value;
    }

    /// <summary>
    /// Explicit conversion từ string sang PostalCode
    /// </summary>
    public static explicit operator PostalCode(string postalCode)
    {
        return Create(postalCode);
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
