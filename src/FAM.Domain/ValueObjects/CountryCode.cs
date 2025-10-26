using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Country Code Value Object - đảm bảo tính toàn vẹn của country code (ISO 3166-1 alpha-2)
/// </summary>
public sealed class CountryCode : ValueObject
{
    public string Value { get; private set; }

    private CountryCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo CountryCode từ string với validation
    /// </summary>
    public static CountryCode Create(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new DomainException("Country code cannot be empty");

        countryCode = countryCode.Trim().ToUpperInvariant();

        if (countryCode.Length != 2)
            throw new DomainException("Country code must be exactly 2 characters");

        if (!IsValidCountryCode(countryCode))
            throw new DomainException($"Invalid country code: {countryCode}");

        return new CountryCode(countryCode);
    }

    /// <summary>
    /// Kiểm tra country code hợp lệ (ISO 3166-1 alpha-2)
    /// </summary>
    private static bool IsValidCountryCode(string code)
    {
        // ISO 3166-1 alpha-2 country codes
        var validCodes = new HashSet<string>
        {
            "AF", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ",
            "BS", "BH", "BD", "BB", "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BA", "BW", "BR", "BN",
            "BG", "BF", "BI", "CV", "KH", "CM", "CA", "KY", "CF", "TD", "CL", "CN", "CO", "KM", "CG",
            "CD", "CK", "CR", "CI", "HR", "CU", "CW", "CY", "CZ", "DK", "DJ", "DM", "DO", "EC", "EG",
            "SV", "GQ", "ER", "EE", "SZ", "ET", "FK", "FO", "FJ", "FI", "FR", "GA", "GM", "GE", "DE",
            "GH", "GI", "GR", "GL", "GD", "GU", "GT", "GG", "GN", "GW", "GY", "HT", "HN", "HK", "HU",
            "IS", "IN", "ID", "IR", "IQ", "IE", "IM", "IL", "IT", "JM", "JP", "JE", "JO", "KZ", "KE",
            "KI", "KP", "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI", "LT", "LU", "MO",
            "MK", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MR", "MU", "MX", "FM", "MD", "MC", "MN",
            "ME", "MS", "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "NC", "NZ", "NI", "NE", "NG", "NU",
            "NF", "MP", "NO", "OM", "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT",
            "PR", "QA", "RO", "RU", "RW", "WS", "SM", "ST", "SA", "SN", "RS", "SC", "SL", "SG", "SX",
            "SK", "SI", "SB", "SO", "ZA", "SS", "ES", "LK", "SD", "SR", "SE", "CH", "SY", "TW", "TJ",
            "TZ", "TH", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "AE",
            "GB", "US", "UY", "UZ", "VU", "VA", "VE", "VN", "VG", "VI", "WF", "EH", "YE", "ZM", "ZW"
        };

        return validCodes.Contains(code);
    }

    /// <summary>
    /// Lấy tên quốc gia từ country code (đơn giản hóa)
    /// </summary>
    public string GetCountryName()
    {
        var countryNames = new Dictionary<string, string>
        {
            ["VN"] = "Vietnam",
            ["US"] = "United States",
            ["GB"] = "United Kingdom",
            ["DE"] = "Germany",
            ["FR"] = "France",
            ["JP"] = "Japan",
            ["CN"] = "China",
            ["KR"] = "South Korea",
            ["SG"] = "Singapore",
            ["TH"] = "Thailand",
            ["MY"] = "Malaysia",
            ["ID"] = "Indonesia",
            ["PH"] = "Philippines",
            ["AU"] = "Australia",
            ["CA"] = "Canada",
            ["BR"] = "Brazil",
            ["MX"] = "Mexico",
            ["AR"] = "Argentina",
            ["CL"] = "Chile",
            ["CO"] = "Colombia",
            ["PE"] = "Peru",
            ["VE"] = "Venezuela",
            ["EC"] = "Ecuador",
            ["UY"] = "Uruguay",
            ["PY"] = "Paraguay",
            ["BO"] = "Bolivia",
            ["GY"] = "Guyana",
            ["SR"] = "Suriname",
            ["FK"] = "Falkland Islands",
            ["GS"] = "South Georgia and the South Sandwich Islands",
            ["AQ"] = "Antarctica",
            ["BV"] = "Bouvet Island",
            ["HM"] = "Heard Island and McDonald Islands",
            ["IO"] = "British Indian Ocean Territory",
            ["TF"] = "French Southern Territories",
            ["UM"] = "United States Minor Outlying Islands"
        };

        return countryNames.GetValueOrDefault(Value, "Unknown");
    }

    /// <summary>
    /// Kiểm tra có phải châu Á không
    /// </summary>
    public bool IsAsian()
    {
        var asianCodes = new[] { "VN", "CN", "JP", "KR", "SG", "TH", "MY", "ID", "PH", "TW", "HK", "MO", "KP", "MM", "KH", "LA", "BN", "TL" };
        return asianCodes.Contains(Value);
    }

    /// <summary>
    /// Kiểm tra có phải châu Âu không
    /// </summary>
    public bool IsEuropean()
    {
        var europeanCodes = new[] { "GB", "DE", "FR", "IT", "ES", "NL", "BE", "AT", "CH", "SE", "NO", "DK", "FI", "PL", "CZ", "SK", "HU", "RO", "BG", "HR", "SI", "EE", "LV", "LT", "MT", "CY", "LU", "IE", "IS", "FO", "GI", "PT", "GR", "TR", "RU", "UA", "BY", "MD", "BA", "ME", "MK", "AL", "RS", "XK" };
        return europeanCodes.Contains(Value);
    }

    /// <summary>
    /// Kiểm tra có phải châu Mỹ không
    /// </summary>
    public bool IsAmerican()
    {
        var americanCodes = new[] { "US", "CA", "MX", "BR", "AR", "CL", "CO", "PE", "VE", "EC", "UY", "PY", "BO", "GY", "SR", "FK", "GS", "AQ", "BV", "HM", "IO", "TF", "UM" };
        return americanCodes.Contains(Value);
    }

    /// <summary>
    /// Implicit conversion từ CountryCode sang string
    /// </summary>
    public static implicit operator string(CountryCode countryCode) => countryCode.Value;

    /// <summary>
    /// Explicit conversion từ string sang CountryCode
    /// </summary>
    public static explicit operator CountryCode(string countryCode) => Create(countryCode);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}