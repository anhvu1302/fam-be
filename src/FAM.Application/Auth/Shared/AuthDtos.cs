namespace FAM.Application.Auth.Shared;

/// <summary>
/// User info DTO (for auth responses - contains safe profile information)
/// </summary>
public record UserInfoDto
{
    public long Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    // Profile Information
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? FullName { get; init; }
    public string? Avatar { get; init; }
    public string? PhoneNumber { get; init; }
    public string? PhoneCountryCode { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Bio { get; init; }

    // Account Status
    public bool IsEmailVerified { get; init; }
    public bool IsTwoFactorEnabled { get; init; }

    // Preferences
    public string? PreferredLanguage { get; init; }
    public string? TimeZone { get; init; }
}

/// <summary>
/// Login response DTO - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.LoginResponse for API responses
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

    /// <summary>
    /// Whether email verification is required to complete login
    /// </summary>
    public bool RequiresEmailVerification { get; set; }

    /// <summary>
    /// Masked email for display (when RequiresEmailVerification is true)
    /// Frontend should use this masked email and ask user to verify the full email
    /// </summary>
    public string? MaskedEmail { get; set; }
}

/// <summary>
/// Verify 2FA response DTO - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.VerifyTwoFactorResponse for API responses
/// </summary>
public class VerifyTwoFactorResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public UserInfoDto User { get; set; } = null!;
}