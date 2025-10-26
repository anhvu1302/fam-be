using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// URL Value Object - đảm bảo tính toàn vẹn của URL trong domain
/// </summary>
public sealed class Url : ValueObject
{
    public string Value { get; private set; }

    private Url(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo URL từ string với validation
    /// </summary>
    public static Url Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("URL cannot be empty");

        url = url.Trim();

        if (!IsValidUrl(url))
            throw new DomainException("Invalid URL format");

        return new Url(url);
    }

    /// <summary>
    /// Kiểm tra format URL hợp lệ
    /// </summary>
    private static bool IsValidUrl(string url)
    {
        if (!url.Contains("://"))
            return false;

        try
        {
            var uri = new Uri(url);
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Lấy domain từ URL
    /// </summary>
    public string GetDomain()
    {
        try
        {
            var uri = new Uri(Value);
            return uri.Host;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Lấy scheme (http/https) từ URL
    /// </summary>
    public string GetScheme()
    {
        try
        {
            var uri = new Uri(Value);
            return uri.Scheme;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Kiểm tra có phải HTTPS không
    /// </summary>
    public bool IsHttps() => GetScheme() == Uri.UriSchemeHttps;

    /// <summary>
    /// Kiểm tra có phải HTTP không
    /// </summary>
    public bool IsHttp() => GetScheme() == Uri.UriSchemeHttp;

    /// <summary>
    /// Implicit conversion từ Url sang string
    /// </summary>
    public static implicit operator string(Url url) => url.Value;

    /// <summary>
    /// Explicit conversion từ string sang Url
    /// </summary>
    public static explicit operator Url(string url) => Create(url);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}