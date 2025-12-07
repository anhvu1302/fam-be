namespace FAM.Application.Auth.Shared;

/// <summary>
/// Response cho forgot password - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.ForgotPasswordResponse for API responses
/// Success/Message/Code are handled by ApiSuccessResponse/ApiErrorResponse wrapper in WebApi layer
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>
    /// Email đã mask để confirm (vd: ab***@gmail.com)
    /// </summary>
    public string? MaskedEmail { get; set; }
}

/// <summary>
/// Response cho reset password - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.ResetPasswordResponse for API responses
/// Success/Message/Code are handled by ApiSuccessResponse/ApiErrorResponse wrapper in WebApi layer
/// </summary>
public class ResetPasswordResponse
{
    // No properties - operation completes successfully or throws error via Result pattern
}

/// <summary>
/// Response cho verify reset token - used internally by Application handlers
/// Mapped to WebApi.Contracts.Auth.VerifyResetTokenResponse for API responses
/// Success/Message/Code are handled by ApiSuccessResponse/ApiErrorResponse wrapper in WebApi layer
/// </summary>
public class VerifyResetTokenResponse
{
    /// <summary>
    /// Email (masked)
    /// </summary>
    public string? MaskedEmail { get; set; }
}