using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Asset Tag Value Object - đảm bảo tính toàn vẹn của asset tag
/// </summary>
public sealed class AssetTag : ValueObject
{
    public string Value { get; private set; }

    private AssetTag(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo AssetTag từ string với validation
    /// </summary>
    public static AssetTag Create(string assetTag)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
            throw new DomainException("Asset tag cannot be empty");

        assetTag = assetTag.Trim();

        if (assetTag.Length > 50)
            throw new DomainException("Asset tag cannot exceed 50 characters");

        return new AssetTag(assetTag);
    }

    /// <summary>
    /// Implicit conversion từ AssetTag sang string
    /// </summary>
    public static implicit operator string(AssetTag assetTag) => assetTag.Value;

    /// <summary>
    /// Explicit conversion từ string sang AssetTag
    /// </summary>
    public static explicit operator AssetTag(string assetTag) => Create(assetTag);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}