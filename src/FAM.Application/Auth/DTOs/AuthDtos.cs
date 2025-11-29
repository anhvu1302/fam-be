namespace FAM.Application.Auth.DTOs;

/// <summary>
/// Login request DTO - supports login with username or email
/// </summary>
public class LoginRequest
{
    public string Identity { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public UserInfoDto User { get; set; } = null!;
    public bool RequiresTwoFactor { get; set; }
    public string? TwoFactorSessionToken { get; set; }
}

/// <summary>
/// Verify 2FA request DTO
/// </summary>
public class VerifyTwoFactorRequest
{
    public string TwoFactorCode { get; set; } = string.Empty;
    public string TwoFactorSessionToken { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

/// <summary>
/// Verify 2FA response DTO
/// </summary>
public class VerifyTwoFactorResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public UserInfoDto User { get; set; } = null!;
}

/// <summary>
/// User info DTO (minimal info for auth responses)
/// </summary>
public record UserInfoDto
{
    public long Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsTwoFactorEnabled { get; init; }
}

/// <summary>
/// Refresh token request DTO
/// </summary>
public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? DeviceId { get; init; }
}

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public bool LogoutAllDevices { get; set; } = false;
}