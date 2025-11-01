using FAM.Application.Auth.DTOs;

namespace FAM.Application.Auth.Services;

/// <summary>
/// Service for JWT token operations
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    string GenerateAccessToken(long userId, string username, string email, IEnumerable<string> roles);
    
    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token);
    
    /// <summary>
    /// Get user ID from token
    /// </summary>
    long GetUserIdFromToken(string token);
}
