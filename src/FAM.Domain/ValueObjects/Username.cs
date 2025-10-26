using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Username Value Object - đảm bảo tính toàn vẹn của username trong domain
/// </summary>
public sealed class Username : ValueObject
{
    public string Value { get; private set; }

    private Username(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo Username từ string với validation
    /// </summary>
    public static Username Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username cannot be empty");

        username = username.Trim();

        if (username.Length < 3)
            throw new DomainException("Username must be at least 3 characters long");

        if (username.Length > 50)
            throw new DomainException("Username cannot exceed 50 characters");

        if (!IsValidUsernameFormat(username))
            throw new DomainException("Username can only contain letters, numbers, underscores, and hyphens");

        return new Username(username);
    }

    /// <summary>
    /// Kiểm tra format username hợp lệ
    /// </summary>
    private static bool IsValidUsernameFormat(string username)
    {
        return username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }

    /// <summary>
    /// Kiểm tra username có an toàn không (không chứa từ khóa nhạy cảm)
    /// </summary>
    public bool IsSafeUsername()
    {
        var unsafeWords = new[] { "admin", "root", "system", "superuser", "god" };
        var lowerUsername = Value.ToLower();

        return !unsafeWords.Any(word => lowerUsername.Contains(word));
    }

    /// <summary>
    /// Implicit conversion từ Username sang string
    /// </summary>
    public static implicit operator string(Username username) => username.Value;

    /// <summary>
    /// Explicit conversion từ string sang Username
    /// </summary>
    public static explicit operator Username(string username) => Create(username);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}