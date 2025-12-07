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

#region Response Records

/// <summary>
/// User info response for auth responses
/// </summary>
public sealed record UserInfoResponse(
    long Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? FullName,
    string? Avatar,
    string? PhoneNumber,
    string? PhoneCountryCode,
    DateTime? DateOfBirth,
    string? Bio,
    bool IsEmailVerified,
    string? PreferredLanguage,
    string? TimeZone,
    bool TwoFactorEnabled
);

/// <summary>
/// Login response
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    UserInfoResponse User,
    bool RequiresTwoFactor,
    string? TwoFactorSessionToken,
    bool RequiresEmailVerification,
    string? MaskedEmail
);

/// <summary>
/// Verify 2FA response
/// </summary>
public sealed record VerifyTwoFactorResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    UserInfoResponse User
);

/// <summary>
/// Refresh token response
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType
);

/// <summary>
/// Logout response
/// </summary>
public sealed record LogoutResponse(
    bool Success,
    string Message
);

/// <summary>
/// Logout all devices response
/// </summary>
public sealed record LogoutAllResponse(
    bool Success,
    string Message,
    int LoggedOutDevices
);

/// <summary>
/// Change password response
/// </summary>
public sealed record ChangePasswordResponse(
    bool Success,
    string Message,
    string Code
);

/// <summary>
/// Enable 2FA response with secret and recovery codes
/// </summary>
public sealed record Enable2FAResponse(
    string Secret,
    string QrCodeUrl,
    List<string> RecoveryCodes,
    string Message
);

/// <summary>
/// Confirm 2FA response
/// </summary>
public sealed record Confirm2FAResponse(
    bool Success,
    string Message,
    List<string> RecoveryCodes
);

/// <summary>
/// Disable 2FA response
/// </summary>
public sealed record Disable2FAResponse(
    bool Success,
    string Message
);

/// <summary>
/// Disable 2FA with backup code response
/// </summary>
public sealed record DisableTwoFactorWithBackupResponse(
    bool Success,
    string Message
);

/// <summary>
/// Select authentication method response
/// Success/Message are handled by ApiSuccessResponse wrapper
/// </summary>
public sealed record SelectAuthenticationMethodResponse(
    string SelectedMethod,
    string? AdditionalInfo,
    DateTime? ExpiresAt
);

/// <summary>
/// Verify email OTP response
/// </summary>
public sealed record VerifyEmailOtpResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    UserInfoResponse User
);

/// <summary>
/// Verify recovery code response
/// </summary>
public sealed record VerifyRecoveryCodeResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    UserInfoResponse User
);

/// <summary>
/// Forgot password response
/// Success/Message/Code are handled by ApiSuccessResponse/ApiErrorResponse wrapper
/// </summary>
public sealed record ForgotPasswordResponse(
    string? MaskedEmail
);

/// <summary>
/// Reset password response
/// Success/Message/Code are handled by ApiSuccessResponse/ApiErrorResponse wrapper
/// </summary>
public sealed record ResetPasswordResponse;

/// <summary>
/// Verify reset token response
/// Success/Message/Code are handled by ApiSuccessResponse/ApiErrorResponse wrapper
/// </summary>
public sealed record VerifyResetTokenResponse(
    string? MaskedEmail
);

/// <summary>
/// Authentication methods response
/// </summary>
public sealed record AuthenticationMethodsResponse(
    bool EmailAuthenticationEnabled,
    string? MaskedEmail,
    bool TwoFactorAuthenticatorEnabled,
    DateTime? TwoFactorSetupDate,
    bool RecoveryCodesConfigured,
    int RemainingRecoveryCodes,
    List<AuthenticationMethodInfo>? AvailableMethods = null
);

/// <summary>
/// Authentication method info
/// </summary>
public sealed record AuthenticationMethodInfo(
    string MethodType,
    string DisplayName,
    bool IsEnabled,
    bool IsPrimary,
    string? AdditionalInfo
);

/// <summary>
/// JWKS (JSON Web Key Set) response
/// </summary>
public sealed record JwksResponse(
    List<JsonWebKey> Keys
);

/// <summary>
/// JSON Web Key
/// </summary>
public sealed record JsonWebKey(
    string Kty,
    string Use,
    string Alg,
    string Kid,
    string N,
    string E
);

#endregion