namespace FAM.Application.Auth.DTOs;

/// <summary>
/// Response DTO chứa thông tin các phương thức xác thực đang bật
/// </summary>
public class AuthenticationMethodsResponse
{
    /// <summary>
    /// Phương thức xác thực qua email có được bật không
    /// </summary>
    public bool EmailAuthenticationEnabled { get; set; }

    /// <summary>
    /// Email được mask để bảo mật (vd: ab***@gmail.com)
    /// </summary>
    public string? MaskedEmail { get; set; }

    /// <summary>
    /// Phương thức xác thực 2FA (Authenticator App) có được bật không
    /// </summary>
    public bool TwoFactorAuthenticatorEnabled { get; set; }

    /// <summary>
    /// Ngày bật 2FA
    /// </summary>
    public DateTime? TwoFactorSetupDate { get; set; }

    /// <summary>
    /// Recovery codes có được cấu hình không
    /// </summary>
    public bool RecoveryCodesConfigured { get; set; }

    /// <summary>
    /// Số lượng recovery codes còn lại (không hiển thị codes)
    /// </summary>
    public int RemainingRecoveryCodes { get; set; }

    /// <summary>
    /// Danh sách phương thức xác thực có sẵn
    /// </summary>
    public List<AuthenticationMethodInfo> AvailableMethods { get; set; } = new();
}

/// <summary>
/// Thông tin chi tiết về từng phương thức xác thực
/// </summary>
public class AuthenticationMethodInfo
{
    /// <summary>
    /// Loại phương thức: "email_otp", "authenticator_app", "recovery_code"
    /// </summary>
    public string MethodType { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị của phương thức
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức này có được bật không
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Phương thức này có phải là primary không
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Thông tin bổ sung (vd: masked email, số recovery codes còn lại)
    /// </summary>
    public string? AdditionalInfo { get; set; }
}

/// <summary>
/// Request để chọn phương thức xác thực khi login
/// </summary>
public class SelectAuthenticationMethodRequest
{
    /// <summary>
    /// Token session 2FA từ bước login ban đầu
    /// </summary>
    public string TwoFactorSessionToken { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức xác thực được chọn: "email_otp", "authenticator_app", "recovery_code"
    /// </summary>
    public string SelectedMethod { get; set; } = string.Empty;
}

/// <summary>
/// Response sau khi chọn phương thức xác thực
/// </summary>
public class SelectAuthenticationMethodResponse
{
    /// <summary>
    /// Có thành công không
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message hướng dẫn user
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức đã chọn
    /// </summary>
    public string SelectedMethod { get; set; } = string.Empty;

    /// <summary>
    /// Thông tin bổ sung (vd: OTP đã gửi tới email nào)
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Thời gian hết hạn của code (cho email OTP)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request để verify OTP gửi qua email
/// </summary>
public class VerifyEmailOtpRequest
{
    /// <summary>
    /// Token session 2FA
    /// </summary>
    public string TwoFactorSessionToken { get; set; } = string.Empty;

    /// <summary>
    /// Mã OTP nhận được qua email
    /// </summary>
    public string EmailOtp { get; set; } = string.Empty;

    /// <summary>
    /// Có nhớ device không
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// Request để verify recovery code
/// </summary>
public class VerifyRecoveryCodeRequest
{
    /// <summary>
    /// Token session 2FA
    /// </summary>
    public string TwoFactorSessionToken { get; set; } = string.Empty;

    /// <summary>
    /// Recovery code
    /// </summary>
    public string RecoveryCode { get; set; } = string.Empty;

    /// <summary>
    /// Có nhớ device không
    /// </summary>
    public bool RememberMe { get; set; }
}