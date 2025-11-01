using MediatR;

namespace FAM.Application.Auth.Commands;

/// <summary>
/// Command to change user password
/// </summary>
public sealed record ChangePasswordCommand : IRequest<Unit>
{
    public long UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public bool LogoutAllDevices { get; init; }
    /// <summary>
    /// Current device ID (fingerprint string) to keep logged in
    /// </summary>
    public string? CurrentDeviceId { get; init; }
}
