using System.Security.Cryptography;
using System.Text;

using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Password Value Object - đảm bảo tính toàn vẹn và bảo mật của password trong domain
/// </summary>
public sealed class Password : ValueObject
{
    public string Hash { get; private set; } = null!;
    public string Salt { get; private set; } = null!;

    // Constructor for EF Core
    private Password()
    {
    }

    private Password(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    /// <summary>
    /// Tạo Password từ plain text với validation và hashing
    /// </summary>
    public static Password Create(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_EMPTY);
        }

        // Validate password strength
        ValidatePasswordStrength(plainPassword);

        // Generate salt and hash
        string salt = GenerateSalt();
        string hash = HashPassword(plainPassword, salt);

        return new Password(hash, salt);
    }

    /// <summary>
    /// Tạo Password từ existing hash và salt (cho database loading)
    /// </summary>
    public static Password FromHash(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_EMPTY);
        }

        if (string.IsNullOrWhiteSpace(salt))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_EMPTY);
        }

        return new Password(hash, salt);
    }

    /// <summary>
    /// Validate password strength requirements.
    /// Uses centralized rules from DomainRules.Password.
    /// </summary>
    private static void ValidatePasswordStrength(string password)
    {
        if (password.Length < DomainRules.Password.MinLength)
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_TOO_SHORT, DomainRules.Password.MinLength);
        }

        if (password.Length > DomainRules.Password.MaxLength)
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_TOO_LONG, DomainRules.Password.MaxLength);
        }

        if (!DomainRules.Password.HasUppercase(password))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_NO_UPPERCASE);
        }

        if (!DomainRules.Password.HasLowercase(password))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_NO_LOWERCASE);
        }

        if (!DomainRules.Password.HasDigit(password))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_NO_NUMBER);
        }

        if (!DomainRules.Password.HasSpecialChar(password))
        {
            throw new DomainException(ErrorCodes.VO_PASSWORD_NO_SPECIAL);
        }
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    public bool Verify(string plainPassword)
    {
        string computedHash = HashPassword(plainPassword, Salt);
        return Hash == computedHash;
    }

    /// <summary>
    /// Generate a random salt
    /// </summary>
    private static string GenerateSalt()
    {
        // Simple salt generation - in production, use a more secure method
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Hash password with salt using PBKDF2
    /// </summary>
    private static string HashPassword(string password, string salt)
    {
        // Simple hashing - in production, use PBKDF2, Argon2, or similar
        using SHA256 sha256 = SHA256.Create();
        string combined = password + salt;
        byte[] bytes = Encoding.UTF8.GetBytes(combined);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Update password with new plain text
    /// </summary>
    public Password Update(string newPlainPassword)
    {
        return Create(newPlainPassword);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Hash;
        yield return Salt;
    }

    public override string ToString()
    {
        return "*****";
        // Never expose password hash
    }
}
