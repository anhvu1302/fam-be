using System.Security.Cryptography;

using FAM.Domain.Common.Base;

namespace FAM.Domain.Users.ValueObjects;

/// <summary>
/// Refresh Token value object
/// </summary>
public sealed class RefreshToken : ValueObject
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    private RefreshToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Refresh token cannot be empty");
        }

        if (token.Length < 32)
        {
            throw new DomainException("Refresh token must be at least 32 characters");
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new DomainException("Refresh token expiry must be in the future");
        }

        return new RefreshToken(token, expiresAt);
    }

    public static RefreshToken Generate(int expiryDays = 7)
    {
        string token = GenerateSecureToken();
        DateTime expiresAt = DateTime.UtcNow.AddDays(expiryDays);
        return new RefreshToken(token, expiresAt);
    }

    private static string GenerateSecureToken()
    {
        // Generate 256-bit random token
        byte[] randomBytes = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Token;
    }
}
