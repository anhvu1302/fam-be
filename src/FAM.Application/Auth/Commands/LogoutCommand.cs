using MediatR;

namespace FAM.Application.Auth.Commands;

/// <summary>
/// Command to logout current device
/// </summary>
public sealed record LogoutCommand : IRequest<Unit>
{
    public string? RefreshToken { get; init; }
    public string? DeviceId { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>
/// Command to logout all devices
/// </summary>
public sealed record LogoutAllDevicesCommand : IRequest<Unit>
{
    public long UserId { get; init; }
    /// <summary>
    /// Device ID to keep logged in (usually current device)
    /// This is the DeviceId string (fingerprint), not the database Guid
    /// </summary>
    public string? ExceptDeviceId { get; init; }
}
