using FAM.Application.Auth.Shared;
using MediatR;

namespace FAM.Application.Auth.VerifyEmailOtp;

/// <summary>
/// Command to verify email OTP during login flow
/// </summary>
public sealed record VerifyEmailOtpLoginCommand : IRequest<VerifyEmailOtpLoginResponse>
{
    /// <summary>
    /// OTP code from email (6 digits)
    /// </summary>
    public string EmailOtp { get; init; } = string.Empty;

    /// <summary>
    /// Email to verify (extracted from token on backend)
    /// </summary>
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Response for email OTP verification during login
/// </summary>
public class VerifyEmailOtpLoginResponse
{
    /// <summary>
    /// Access token for authenticated requests
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for token renewal
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiry in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Verified user information
    /// </summary>
    public UserInfoDto User { get; set; } = null!;

    /// <summary>
    /// Whether 2FA is required
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// 2FA session token (if RequiresTwoFactor is true)
    /// </summary>
    public string? TwoFactorSessionToken { get; set; }
}