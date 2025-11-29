using FAM.Application.Auth.DTOs;
using MediatR;

namespace FAM.Application.Auth.Commands;

public sealed record Enable2FACommand : IRequest<Enable2FAResponse>
{
    public required long UserId { get; init; }
    public required string Password { get; init; }
}