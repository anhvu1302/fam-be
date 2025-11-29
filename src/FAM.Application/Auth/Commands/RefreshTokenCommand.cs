using FAM.Application.Auth.DTOs;
using MediatR;

namespace FAM.Application.Auth.Commands;

/// <summary>
/// Command to refresh access token
/// </summary>
public sealed record RefreshTokenCommand : IRequest<LoginResponse>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? Location { get; init; }
}