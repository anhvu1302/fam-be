namespace FAM.Application.Auth.DTOs;

public sealed record Enable2FARequest
{
    public required string Password { get; init; }
}
