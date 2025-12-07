using FAM.Application.Auth.Shared;
using MediatR;

namespace FAM.Application.Auth.Login;

/// <summary>
/// Command to login with username/email and password
/// </summary>
public sealed record LoginCommand : IRequest<LoginResponse>
{
    public string Identity { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string? DeviceName { get; init; }
    public string? DeviceType { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Location { get; init; }
    public bool RememberMe { get; init; }
}