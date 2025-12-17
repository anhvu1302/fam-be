using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.Auth;

#region Request Records

/// <summary>
/// Login request - Validated by LoginRequestValidator using DomainRules
/// Supports login with either username or email
/// </summary>
[SwaggerSchema(Required = new[] { "identity", "password" })]
public sealed record LoginRequest(
    string Identity,
    string Password,
    bool RememberMe = false
);

/// <summary>
/// Verify 2FA request - Validated by VerifyTwoFactorRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "twoFactorCode", "twoFactorSessionToken" })]
public sealed record VerifyTwoFactorRequest(
    string TwoFactorCode,
    string TwoFactorSessionToken,
    bool RememberMe = false
);

/// <summary>
/// Refresh token request - Validated by RefreshTokenRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "refreshToken" })]
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
[SwaggerSchema(Required = new[] { "currentPassword", "newPassword" })]
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    bool LogoutAllDevices = false
);

/// <summary>
/// Enable 2FA request - Validated by Enable2FARequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "password" })]
public sealed record Enable2FARequest(
    string Password
);

/// <summary>
/// Confirm 2FA request - Validated by Confirm2FARequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "secret", "code" })]
public sealed record Confirm2FARequest(
    string Secret,
    string Code
);

/// <summary>
/// Disable 2FA request - Validated by Disable2FARequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "password" })]
public sealed record Disable2FARequest(
    string Password
);

/// <summary>
/// Disable 2FA with backup code request - Validated by DisableTwoFactorWithBackupRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "username", "password", "backupCode" })]
public sealed record DisableTwoFactorWithBackupRequest(
    string Username,
    string Password,
    string BackupCode
);

/// <summary>
/// Select authentication method request - Validated by SelectAuthenticationMethodRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "twoFactorSessionToken", "selectedMethod" })]
public sealed record SelectAuthenticationMethodRequest(
    string TwoFactorSessionToken,
    string SelectedMethod
);

/// <summary>
/// Verify email OTP request - Validated by VerifyEmailOtpRequestValidator
/// Used during login flow when email needs verification
/// </summary>
[SwaggerSchema(Required = new[] { "email", "emailOtp" })]
public sealed record VerifyEmailOtpRequest(
    string Email,
    string EmailOtp
);

/// <summary>
/// Verify recovery code request - Validated by VerifyRecoveryCodeRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "twoFactorSessionToken", "recoveryCode" })]
public sealed record VerifyRecoveryCodeRequest(
    string TwoFactorSessionToken,
    string RecoveryCode,
    bool RememberMe = false
);

/// <summary>
/// Forgot password request - Validated by ForgotPasswordRequestValidator using DomainRules
/// </summary>
[SwaggerSchema(Required = new[] { "email" })]
public sealed record ForgotPasswordRequest(
    string Email
);

/// <summary>
/// Reset password request - Validated by ResetPasswordRequestValidator using DomainRules
/// </summary>
[SwaggerSchema(Required = new[] { "email", "resetToken", "newPassword", "confirmPassword" })]
public sealed record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword
);

/// <summary>
/// Verify reset token request - Validated by VerifyResetTokenRequestValidator
/// </summary>
[SwaggerSchema(Required = new[] { "email", "resetToken" })]
public sealed record VerifyResetTokenRequest(
    string Email,
    string ResetToken
);

#endregion
