using System.ComponentModel.DataAnnotations;

namespace FAM.WebApi.Models.Auth;

/// <summary>
/// Login request model - Web API shape validation only
/// </summary>
public class LoginRequestModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

/// <summary>
/// Verify 2FA request model
/// </summary>
public class VerifyTwoFactorRequestModel
{
    [Required(ErrorMessage = "Two-factor code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Two-factor code must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Two-factor code must contain only digits")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Two-factor session token is required")]
    public string TwoFactorSessionToken { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

/// <summary>
/// Refresh token request model
/// </summary>
public class RefreshTokenRequestModel
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;

    public string? DeviceId { get; set; }
}

/// <summary>
/// Change password request model
/// </summary>
public class ChangePasswordRequestModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;

    public bool LogoutAllDevices { get; set; }
}

/// <summary>
/// Enable 2FA request model
/// </summary>
public class Enable2FARequestModel
{
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Confirm 2FA request model
/// </summary>
public class Confirm2FARequestModel
{
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must contain only digits")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Disable 2FA request model
/// </summary>
public class Disable2FARequestModel
{
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Disable 2FA with backup code request model
/// </summary>
public class DisableTwoFactorWithBackupRequestModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Backup code is required")]
    public string BackupCode { get; set; } = string.Empty;
}
