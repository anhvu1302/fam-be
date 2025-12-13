using MediatR;

namespace FAM.Application.Auth.SendEmailVerificationOtp;

/// <summary>
/// Command to send email verification OTP during login flow
/// </summary>
public sealed record SendEmailVerificationOtpCommand : IRequest<SendEmailVerificationOtpResponse>
{
    /// <summary>
    /// Email address to send OTP to
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Session token from login (optional, for traceability)
    /// </summary>
    public string? SessionToken { get; init; }

    /// <summary>
    /// Device identifier (optional, for logging)
    /// </summary>
    public string? DeviceId { get; init; }
}

/// <summary>
/// Response for send email verification OTP
/// </summary>
public class SendEmailVerificationOtpResponse
{
    /// <summary>
    /// OTP session token to use for verify-email-otp endpoint
    /// </summary>
    public string OtpSessionToken { get; set; } = string.Empty;

    /// <summary>
    /// Email that OTP was sent to (masked)
    /// </summary>
    public string MaskedEmail { get; set; } = string.Empty;

    /// <summary>
    /// Expiry time of OTP in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Message for UI
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
