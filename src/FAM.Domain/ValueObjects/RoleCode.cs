using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Role Code Value Object - đảm bảo tính toàn vẹn của role code
/// </summary>
public sealed class RoleCode : ValueObject
{
    public string Value { get; private set; }

    private RoleCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo RoleCode từ string với validation
    /// </summary>
    public static RoleCode Create(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new DomainException("Role code cannot be empty");

        roleCode = roleCode.Trim().ToUpperInvariant();

        if (roleCode.Length > 20)
            throw new DomainException("Role code cannot exceed 20 characters");

        if (!System.Text.RegularExpressions.Regex.IsMatch(roleCode, @"^[A-Z0-9_]+$"))
            throw new DomainException("Role code can only contain uppercase letters, numbers, and underscores");

        return new RoleCode(roleCode);
    }

    /// <summary>
    /// Implicit conversion từ RoleCode sang string
    /// </summary>
    public static implicit operator string(RoleCode roleCode) => roleCode.Value;

    /// <summary>
    /// Explicit conversion từ string sang RoleCode
    /// </summary>
    public static explicit operator RoleCode(string roleCode) => Create(roleCode);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}