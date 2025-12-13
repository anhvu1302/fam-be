using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho số điện thoại
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }
    public string? CountryCode { get; }

    private PhoneNumber(string value, string? countryCode = null)
    {
        Value = value;
        CountryCode = countryCode;
    }

    public static PhoneNumber Create(string phoneNumber, string? countryCode = "+84")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException(ErrorCodes.VO_PHONE_EMPTY);

        // Remove all non-digit characters
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (cleaned.Length < 9 || cleaned.Length > 15)
            throw new DomainException(ErrorCodes.VO_PHONE_INVALID);

        // Use default country code if null
        var finalCountryCode = countryCode ?? "+84";

        return new PhoneNumber(cleaned, finalCountryCode);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }

    public override string ToString()
    {
        return $"{CountryCode}{Value}";
    }

    public string ToFormattedString()
    {
        // Format for Vietnamese phone: 0xxx xxx xxx
        if (Value.Length == 10 && Value.StartsWith("0"))
            return $"{Value.Substring(0, 4)} {Value.Substring(4, 3)} {Value.Substring(7)}";

        return Value;
    }
}
