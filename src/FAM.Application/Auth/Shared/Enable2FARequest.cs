namespace FAM.Application.Auth.Shared;

public sealed record Enable2FARequest
{
    public required string Password { get; init; }
}