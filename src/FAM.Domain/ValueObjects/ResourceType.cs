using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Resource Type Value Object - đảm bảo tính toàn vẹn của resource type
/// </summary>
public sealed class ResourceType : ValueObject
{
    public string Value { get; private set; }

    // Constructor for EF Core
    private ResourceType()
    {
        Value = string.Empty;
    }

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
        {
            throw new DomainException(ErrorCodes.VO_RESOURCE_TYPE_EMPTY);
        }

        resourceType = resourceType.Trim();

        if (resourceType.Length > 50)
        {
            throw new DomainException(ErrorCodes.VO_RESOURCE_TYPE_TOO_LONG);
        }

        return new ResourceType(resourceType);
    }

    /// <summary>
    /// Implicit conversion từ ResourceType sang string
    /// </summary>
    public static implicit operator string(ResourceType resourceType)
    {
        return resourceType.Value;
    }

    /// <summary>
    /// Explicit conversion từ string sang ResourceType
    /// </summary>
    public static explicit operator ResourceType(string resourceType)
    {
        return Create(resourceType);
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
