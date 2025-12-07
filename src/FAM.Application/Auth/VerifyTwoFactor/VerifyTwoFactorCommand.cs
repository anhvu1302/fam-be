using FAM.Application.Auth.Shared;
using MediatR;

namespace FAM.Application.Auth.VerifyTwoFactor;

/// <summary>
/// Command to verify 2FA code
/// </summary>
public sealed record VerifyTwoFactorCommand : IRequest<VerifyTwoFactorResponse>
{
    public string TwoFactorSessionToken { get; init; } = string.Empty;
    public string TwoFactorCode { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string? DeviceName { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Location { get; init; }
    public bool RememberMe { get; init; }
}