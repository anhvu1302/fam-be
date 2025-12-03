using FAM.Application.Auth.Shared;
using MediatR;

namespace FAM.Application.Auth.RefreshToken;

/// <summary>
/// Command to refresh access token
/// </summary>
public sealed record RefreshTokenCommand : IRequest<LoginResponse>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? Location { get; init; }
}