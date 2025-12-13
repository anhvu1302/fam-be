using MediatR;

namespace FAM.Application.Auth.Logout;

/// <summary>
/// Command to logout current device
/// </summary>
public sealed record LogoutCommand : IRequest<Unit>
{
    public string? RefreshToken { get; init; }
    public string? DeviceId { get; init; }
    public string? IpAddress { get; init; }

    /// <summary>
    /// Access token to blacklist (invalidate immediately)
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Access token expiration time (UTC)
    /// If not provided, token will still be blacklisted but with minimal TTL
    /// </summary>
    public DateTime? AccessTokenExpiration { get; init; }
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
