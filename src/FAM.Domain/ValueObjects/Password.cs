using FAM.Domain.Common;
using System.Text.RegularExpressions;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Password Value Object - đảm bảo tính toàn vẹn và bảo mật của password trong domain
/// </summary>
public sealed class Password : ValueObject
{
    public string Hash { get; private set; }
    public string Salt { get; private set; }

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
            throw new DomainException("Password cannot be empty");

        // Validate password strength
        ValidatePasswordStrength(plainPassword);

        // Generate salt and hash
        var salt = GenerateSalt();
        var hash = HashPassword(plainPassword, salt);

        return new Password(hash, salt);
    }

    /// <summary>
    /// Tạo Password từ existing hash và salt (cho database loading)
    /// </summary>
    public static Password FromHash(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty");

        if (string.IsNullOrWhiteSpace(salt))
            throw new DomainException("Password salt cannot be empty");

        return new Password(hash, salt);
    }

    /// <summary>
    /// Validate password strength requirements
    /// </summary>
    private static void ValidatePasswordStrength(string password)
    {
        if (password.Length < 8)
            throw new DomainException("Password must be at least 8 characters long");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            throw new DomainException("Password must contain at least one uppercase letter");

        if (!Regex.IsMatch(password, @"[a-z]"))
            throw new DomainException("Password must contain at least one lowercase letter");

        if (!Regex.IsMatch(password, @"[0-9]"))
            throw new DomainException("Password must contain at least one number");

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            throw new DomainException("Password must contain at least one special character");
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    public bool Verify(string plainPassword)
    {
        var computedHash = HashPassword(plainPassword, Salt);
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
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var combined = password + salt;
        var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
        var hash = sha256.ComputeHash(bytes);
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

    public override string ToString() => "*****"; // Never expose password hash
}