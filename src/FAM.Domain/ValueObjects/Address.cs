using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho địa chỉ
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string? Ward { get; }
    public string? District { get; }
    public string City { get; }
    public string? Province { get; }
    public string CountryCode { get; }
    public string? PostalCode { get; }

    private Address(
        string street,
        string city,
        string countryCode,
        string? ward = null,
        string? district = null,
        string? province = null,
        string? postalCode = null)
    {
        Street = street;
        Ward = ward;
        District = district;
        City = city;
        Province = province;
        CountryCode = countryCode;
        PostalCode = postalCode;
    }

    public static Address Create(
        string street,
        string city,
        string countryCode,
        string? ward = null,
        string? district = null,
        string? province = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException(ErrorCodes.VO_ADDRESS_STREET_EMPTY);

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException(ErrorCodes.VO_ADDRESS_CITY_EMPTY);

        if (string.IsNullOrWhiteSpace(countryCode))
            throw new DomainException(ErrorCodes.VO_ADDRESS_COUNTRY_EMPTY);

        return new Address(street, city, countryCode.ToUpperInvariant(), ward, district, province, postalCode);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Ward;
        yield return District;
        yield return City;
        yield return Province;
        yield return CountryCode;
        yield return PostalCode;
    }

    public override string ToString()
    {
        IEnumerable<string?> parts = new[] { Street, Ward, District, City, Province, CountryCode, PostalCode }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }
}
