using FAM.Application.Auth.DTOs;
using MediatR;

namespace FAM.Application.Auth.Commands;

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

    public static VerifyTwoFactorCommand From(VerifyTwoFactorRequest request, string deviceId, string deviceName,
        string deviceType, string? ipAddress, string? userAgent, string? location)
    {
        return new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = request.TwoFactorSessionToken,
            TwoFactorCode = request.TwoFactorCode,
            DeviceId = deviceId,
            DeviceName = deviceName,
            DeviceType = deviceType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Location = location,
            RememberMe = request.RememberMe
        };
    }
}