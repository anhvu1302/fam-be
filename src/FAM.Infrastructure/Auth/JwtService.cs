using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FAM.Application.Auth.DTOs;
using FAM.Application.Auth.Services;
using Microsoft.IdentityModel.Tokens;

namespace FAM.Infrastructure.Auth;

/// <summary>
/// JWT service implementation
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;

    public JwtService()
    {
        // Load from environment variables
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET")
                  ?? throw new InvalidOperationException("JWT_SECRET environment variable not configured");
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "FAM.API";
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "FAM.Client";

        var expiryMinutesStr = Environment.GetEnvironmentVariable("ACCESS_TOKEN_EXPIRATION_MINUTES");
        _accessTokenExpiryMinutes =
            !string.IsNullOrEmpty(expiryMinutesStr) && int.TryParse(expiryMinutesStr, out var minutes)
                ? minutes
                : 60;

        if (_secret.Length < 32)
            throw new InvalidOperationException("JWT_SECRET must be at least 32 characters");
    }

    public string GenerateAccessToken(long userId, string username, string email, IEnumerable<string> roles)
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

        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
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
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (long.TryParse(userIdClaim, out var userId)) return userId;

            throw new InvalidOperationException("User ID not found in token");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid token", ex);
        }
    }
}