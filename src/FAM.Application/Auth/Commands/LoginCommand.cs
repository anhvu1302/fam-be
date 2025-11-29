using FAM.Application.Auth.DTOs;
using MediatR;

namespace FAM.Application.Auth.Commands;

/// <summary>
/// Command to login with username and password
/// </summary>
public sealed record LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string? DeviceName { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Location { get; init; }
    public bool RememberMe { get; init; }

    public static LoginCommand From(LoginRequest request, string deviceId, string deviceName, string deviceType,
        string? ipAddress, string? userAgent)
    {
        return new LoginCommand
        {
            Username = request.Username,
            Password = request.Password,
            DeviceId = deviceId,
            DeviceName = deviceName,
            DeviceType = deviceType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RememberMe = request.RememberMe
        };
    }
}