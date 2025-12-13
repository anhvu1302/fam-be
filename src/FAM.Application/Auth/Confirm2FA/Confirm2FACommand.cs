using FAM.Application.Auth.Shared;

using MediatR;

namespace FAM.Application.Auth.Confirm2FA;

public sealed record Confirm2FACommand : IRequest<Confirm2FAResponse>
{
    public required long UserId { get; init; }
    public required string Secret { get; init; }
    public required string Code { get; init; }
}
