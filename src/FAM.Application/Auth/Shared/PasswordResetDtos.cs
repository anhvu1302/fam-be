namespace FAM.Application.Auth.Shared;

/// <summary>
/// Request để gửi email reset password
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email của user cần reset password
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Response cho forgot password
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>
    /// Có thành công không
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message thông báo
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email đã mask để confirm (vd: ab***@gmail.com)
    /// </summary>
    public string? MaskedEmail { get; set; }
}

/// <summary>
/// Request để reset password với token
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Email của user
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Reset token nhận được qua email
    /// </summary>
    public string ResetToken { get; set; } = string.Empty;

    /// <summary>
    /// Password mới
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm password mới
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response cho reset password
/// </summary>
public class ResetPasswordResponse
{
    /// <summary>
    /// Có thành công không
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message thông báo
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request để verify reset token (check token còn valid không trước khi user nhập password)
/// </summary>
public class VerifyResetTokenRequest
{
    /// <summary>
    /// Email của user
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Reset token cần verify
    /// </summary>
    public string ResetToken { get; set; } = string.Empty;
}

/// <summary>
/// Response cho verify reset token
/// </summary>
public class VerifyResetTokenResponse
{
    /// <summary>
    /// Token có valid không
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Message thông báo
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email (masked)
    /// </summary>
    public string? MaskedEmail { get; set; }
}