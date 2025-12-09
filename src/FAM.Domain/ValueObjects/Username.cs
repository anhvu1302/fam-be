using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Username Value Object - đảm bảo tính toàn vẹn của username trong domain.
/// Validation rules are centralized in DomainRules.Username.
/// </summary>
public sealed class Username : ValueObject
{
    public string Value { get; private set; }

    private Username(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo Username từ string với validation.
    /// Throws DomainException with error code và details cho i18n.
    /// Validation rules được định nghĩa trong DomainRules.Username.
    /// </summary>
    public static Username Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException(ErrorCodes.VO_USERNAME_EMPTY, new
            {
                field = "Username"
            });

        username = username.Trim();

        if (username.Length < DomainRules.Username.MinLength)
            throw new DomainException(ErrorCodes.VO_USERNAME_TOO_SHORT, new
            {
                field = "Username",
                minLength = DomainRules.Username.MinLength,
                actualLength = username.Length
            });

        if (username.Length > DomainRules.Username.MaxLength)
            throw new DomainException(ErrorCodes.VO_USERNAME_TOO_LONG, new
            {
                field = "Username",
                maxLength = DomainRules.Username.MaxLength,
                actualLength = username.Length
            });

        if (!DomainRules.Username.IsValidFormat(username))
            throw new DomainException(ErrorCodes.VO_USERNAME_INVALID, new
            {
                field = "Username",
                allowedChars = DomainRules.Username.AllowedCharacters,
                pattern = DomainRules.Username.Pattern
            });

        return new Username(username);
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
    public static implicit operator string(Username username)
    {
        return username.Value;
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