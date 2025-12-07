namespace FAM.Application.Auth.Shared;

/// <summary>
/// Response cho forgot password - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.ForgotPasswordResponse for API responses
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>
    /// Có thành công không
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error/Success code cho i18n (FE dùng để dịch thuật)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Message thông báo (tiếng Anh mặc định)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email đã mask để confirm (vd: ab***@gmail.com)
    /// </summary>
    public string? MaskedEmail { get; set; }
}

/// <summary>
/// Response cho reset password - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.ResetPasswordResponse for API responses
/// </summary>
public class ResetPasswordResponse
{
    /// <summary>
    /// Có thành công không
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error/Success code cho i18n (FE dùng để dịch thuật)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Message thông báo (tiếng Anh mặc định)
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response cho verify reset token - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.VerifyResetTokenResponse for API responses
/// </summary>
public class VerifyResetTokenResponse
{
    /// <summary>
    /// Token có valid không
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error/Success code cho i18n (FE dùng để dịch thuật)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Message thông báo (tiếng Anh mặc định)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email (masked)
    /// </summary>
    public string? MaskedEmail { get; set; }
}