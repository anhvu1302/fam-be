namespace FAM.Application.Auth.Services;

/// <summary>
/// Service for JWT token operations using RSA asymmetric signing
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token using RSA (asymmetric key) with specified key ID
    /// </summary>
    string GenerateAccessTokenWithRsa(
        long userId,
        string username,
        string email,
        IEnumerable<string> roles,
        string keyId,
        string privateKeyPem,
        string algorithm = "RS256");

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token with RSA public key
    /// </summary>
    bool ValidateTokenWithRsa(string token, string publicKeyPem);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    long GetUserIdFromToken(string token);

    /// <summary>
    /// Get Key ID (kid) from token header
    /// </summary>
    string? GetKeyIdFromToken(string token);

    /// <summary>
    /// Get JTI (JWT ID) from token claims
    /// Used to track and revoke specific access tokens
    /// </summary>
    string? GetJtiFromToken(string token);

    /// <summary>
    /// Get configured access token expiry time in minutes
    /// </summary>
    int AccessTokenExpiryMinutes { get; }

    /// <summary>
    /// Get configured refresh token expiry time in days
    /// </summary>
    int RefreshTokenExpiryDays { get; }
}
