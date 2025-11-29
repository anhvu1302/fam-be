using FAM.Application.Auth.DTOs;

namespace FAM.Application.Auth.Services;

/// <summary>
/// Service for JWT token operations
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token using HMAC (symmetric key)
    /// </summary>
    string GenerateAccessToken(long userId, string username, string email, IEnumerable<string> roles);

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
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token);

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
}