using FAM.Application.Auth.DTOs;
using MediatR;

namespace FAM.Application.Auth.Commands;

/// <summary>
/// Command để chọn phương thức xác thực (email OTP, authenticator app, recovery code)
/// </summary>
public sealed record SelectAuthenticationMethodCommand : IRequest<SelectAuthenticationMethodResponse>
{
    /// <summary>
    /// Token session 2FA từ bước login ban đầu
    /// </summary>
    public string TwoFactorSessionToken { get; init; } = string.Empty;

    /// <summary>
    /// Phương thức được chọn: "email_otp", "authenticator_app", "recovery_code"
    /// </summary>
    public string SelectedMethod { get; init; } = string.Empty;
}

/// <summary>
/// Command để verify email OTP
/// </summary>
public sealed record VerifyEmailOtpCommand : IRequest<VerifyTwoFactorResponse>
{
    public string TwoFactorSessionToken { get; init; } = string.Empty;
    public string EmailOtp { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string? DeviceName { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Location { get; init; }
}

/// <summary>
/// Command để verify recovery code
/// </summary>
public sealed record VerifyRecoveryCodeCommand : IRequest<VerifyTwoFactorResponse>
{
    public string TwoFactorSessionToken { get; init; } = string.Empty;
    public string RecoveryCode { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string? DeviceName { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Location { get; init; }
}