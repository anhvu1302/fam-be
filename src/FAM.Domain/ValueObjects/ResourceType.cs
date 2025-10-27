using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Resource Type Value Object - đảm bảo tính toàn vẹn của resource type
/// </summary>
public sealed class ResourceType : ValueObject
{
    public string Value { get; private set; }

    private ResourceType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo ResourceType từ string với validation
    /// </summary>
    public static ResourceType Create(string resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new DomainException("Resource type cannot be empty");

        resourceType = resourceType.Trim();

        if (resourceType.Length > 50)
            throw new DomainException("Resource type cannot exceed 50 characters");

        return new ResourceType(resourceType);
    }

    /// <summary>
    /// Implicit conversion từ ResourceType sang string
    /// </summary>
    public static implicit operator string(ResourceType resourceType) => resourceType.Value;

    /// <summary>
    /// Explicit conversion từ string sang ResourceType
    /// </summary>
    public static explicit operator ResourceType(string resourceType) => Create(resourceType);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}