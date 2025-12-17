using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using FAM.Domain.Abstractions;

using Microsoft.IdentityModel.Tokens;

namespace FAM.Infrastructure.Auth;

/// <summary>
/// JWT service implementation using RSA (asymmetric) signing with keys from database
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public JwtService()
    {
        // Load from environment variables (fallback for backward compatibility)
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "FAM.API";
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "FAM.Client";

        var expiryMinutesStr = Environment.GetEnvironmentVariable("ACCESS_TOKEN_EXPIRATION_MINUTES");
        _accessTokenExpiryMinutes =
            !string.IsNullOrEmpty(expiryMinutesStr) && int.TryParse(expiryMinutesStr, out var minutes)
                ? minutes
                : 60;

        var refreshDaysStr = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRATION_DAYS");
        _refreshTokenExpiryDays =
            !string.IsNullOrEmpty(refreshDaysStr) && int.TryParse(refreshDaysStr, out var days)
                ? days
                : 30;
    }

    public int AccessTokenExpiryMinutes => _accessTokenExpiryMinutes;
    public int RefreshTokenExpiryDays => _refreshTokenExpiryDays;

    /// <summary>
    /// Generate access token using RSA (asymmetric key)
    /// </summary>
    public string GenerateAccessTokenWithRsa(
        long userId,
        string username,
        string email,
        IEnumerable<string> roles,
        string keyId,
        string privateKeyPem,
        string algorithm = "RS256")
    {
        List<Claim> claims = CreateClaims(userId, username, email, roles);

        var rsa = RSA.Create(); // Don't dispose - key must live until token is written
        rsa.ImportFromPem(privateKeyPem);

        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            KeyId = keyId
        };

        var signingAlgorithm = algorithm switch
        {
            "RS256" => SecurityAlgorithms.RsaSha256,
            "RS384" => SecurityAlgorithms.RsaSha384,
            "RS512" => SecurityAlgorithms.RsaSha512,
            _ => SecurityAlgorithms.RsaSha256
        };

        var credentials = new SigningCredentials(rsaSecurityKey, signingAlgorithm);

        var tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken? token = tokenHandler.CreateJwtSecurityToken(
            _issuer,
            _audience,
            new ClaimsIdentity(claims),
            null,
            DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            DateTime.UtcNow,
            credentials
        );

        // Explicitly set the kid in the header
        token.Header["kid"] = keyId;

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Validate token using RSA public key
    /// </summary>
    public bool ValidateTokenWithRsa(string token, string publicKeyPem)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            var rsaSecurityKey = new RsaSecurityKey(rsa);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = rsaSecurityKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public long GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (long.TryParse(userIdClaim, out var userId)) return userId;

            throw new InvalidOperationException("User ID not found in token");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid token", ex);
        }
    }

    /// <summary>
    /// Get Key ID (kid) from JWT token header
    /// </summary>
    public string? GetKeyIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Header.Kid;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get JTI (JWT ID) from JWT token claims
    /// </summary>
    public string? GetJtiFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
            Claim? jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            return jtiClaim?.Value;
        }
        catch
        {
            return null;
        }
    }

    #region Helper Methods

    private List<Claim> CreateClaims(long userId, string username, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }

    #endregion
}
