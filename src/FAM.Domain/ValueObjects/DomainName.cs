using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Domain Value Object - đảm bảo tính toàn vẹn của domain name
/// </summary>
public sealed class DomainName : ValueObject
{
    public string Value { get; private set; }

    private DomainName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo DomainName từ string với validation
    /// </summary>
    public static DomainName Create(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new DomainException(ErrorCodes.VO_DOMAIN_EMPTY);

        domain = domain.Trim().ToLowerInvariant();

        if (domain.Length > 253)
            throw new DomainException(ErrorCodes.VO_DOMAIN_TOO_LONG);

        // Basic domain validation
        if (!System.Text.RegularExpressions.Regex.IsMatch(domain,
                @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?$"))
            throw new DomainException(ErrorCodes.VO_DOMAIN_INVALID);

        return new DomainName(domain);
    }

    /// <summary>
    /// Implicit conversion từ DomainName sang string
    /// </summary>
    public static implicit operator string(DomainName domainName)
    {
        return domainName.Value;
    }

    /// <summary>
    /// Explicit conversion từ string sang DomainName
    /// </summary>
    public static explicit operator DomainName(string domain)
    {
        return Create(domain);
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