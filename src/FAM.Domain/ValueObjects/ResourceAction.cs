using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Resource Action Value Object - đảm bảo tính toàn vẹn của resource action
/// </summary>
public sealed class ResourceAction : ValueObject
{
    public string Value { get; private set; }

    private ResourceAction(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo ResourceAction từ string với validation
    /// </summary>
    public static ResourceAction Create(string resourceAction)
    {
        if (string.IsNullOrWhiteSpace(resourceAction))
            throw new DomainException("Resource action cannot be empty");

        resourceAction = resourceAction.Trim();

        if (resourceAction.Length > 50)
            throw new DomainException("Resource action cannot exceed 50 characters");

        return new ResourceAction(resourceAction);
    }

    /// <summary>
    /// Implicit conversion từ ResourceAction sang string
    /// </summary>
    public static implicit operator string(ResourceAction resourceAction) => resourceAction.Value;

    /// <summary>
    /// Explicit conversion từ string sang ResourceAction
    /// </summary>
    public static explicit operator ResourceAction(string resourceAction) => Create(resourceAction);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}