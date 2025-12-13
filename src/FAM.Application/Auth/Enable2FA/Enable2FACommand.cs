using FAM.Application.Auth.Shared;

using MediatR;

namespace FAM.Application.Auth.Enable2FA;

public sealed record Enable2FACommand : IRequest<Enable2FAResponse>
{
    public required long UserId { get; init; }
    public required string Password { get; init; }
}
