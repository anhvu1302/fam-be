using MediatR;

namespace FAM.Application.Auth.Disable2FA;

public sealed record Disable2FACommand : IRequest<bool>
{
    public required long UserId { get; init; }
    public required string Password { get; init; }
}