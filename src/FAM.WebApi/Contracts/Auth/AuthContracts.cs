namespace FAM.WebApi.Contracts.Auth;

#region Request Records

/// <summary>
/// Login request - Validated by LoginRequestValidator using DomainRules
/// Supports login with either username or email
/// </summary>
public sealed record LoginRequest(
    string Identity,
    string Password,
    bool RememberMe = false
);

/// <summary>
/// Verify 2FA request - Validated by VerifyTwoFactorRequestValidator
/// </summary>
public sealed record VerifyTwoFactorRequest(
    string TwoFactorCode,
    string TwoFactorSessionToken,
    bool RememberMe = false
);

/// <summary>
/// Refresh token request - Validated by RefreshTokenRequestValidator
/// </summary>
public sealed record RefreshTokenRequest(
    string RefreshToken,
    string? DeviceId = null
);

/// <summary>
/// Logout all devices request - Validated by LogoutAllRequestValidator
/// </summary>
public sealed record LogoutAllRequest(
    bool ExceptCurrentDevice = false
);

/// <summary>
/// Change password request - Validated by ChangePasswordRequestValidator using DomainRules
/// </summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    bool LogoutAllDevices = false
);

/// <summary>
/// Enable 2FA request - Validated by Enable2FARequestValidator
/// </summary>
public sealed record Enable2FARequest(
    string Password
);

/// <summary>
/// Confirm 2FA request - Validated by Confirm2FARequestValidator
/// </summary>
public sealed record Confirm2FARequest(
    string Secret,
    string Code
);

/// <summary>
/// Disable 2FA request - Validated by Disable2FARequestValidator
/// </summary>
public sealed record Disable2FARequest(
    string Password
);

/// <summary>
/// Disable 2FA with backup code request - Validated by DisableTwoFactorWithBackupRequestValidator
/// </summary>
public sealed record DisableTwoFactorWithBackupRequest(
    string Username,
    string Password,
    string BackupCode
);

/// <summary>
/// Select authentication method request - Validated by SelectAuthenticationMethodRequestValidator
/// </summary>
public sealed record SelectAuthenticationMethodRequest(
    string TwoFactorSessionToken,
    string SelectedMethod
);

/// <summary>
/// Verify email OTP request - Validated by VerifyEmailOtpRequestValidator
/// Used during login flow when email needs verification
/// </summary>
public sealed record VerifyEmailOtpRequest(
    [property: System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email is required")]
    string Email,
    
    [property: System.ComponentModel.DataAnnotations.Required(ErrorMessage = "OTP code is required")]
    string EmailOtp
);

/// <summary>
/// Verify recovery code request - Validated by VerifyRecoveryCodeRequestValidator
/// </summary>
public sealed record VerifyRecoveryCodeRequest(
    string TwoFactorSessionToken,
    string RecoveryCode,
    bool RememberMe = false
);

/// <summary>
/// Forgot password request - Validated by ForgotPasswordRequestValidator using DomainRules
/// </summary>
public sealed record ForgotPasswordRequest(
    string Email
);

/// <summary>
/// Reset password request - Validated by ResetPasswordRequestValidator using DomainRules
/// </summary>
public sealed record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword
);

/// <summary>
/// Verify reset token request - Validated by VerifyResetTokenRequestValidator
/// </summary>
public sealed record VerifyResetTokenRequest(
    string Email,
    string ResetToken
);

#endregion
