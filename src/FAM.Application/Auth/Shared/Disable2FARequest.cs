namespace FAM.Application.Auth.Shared;

public sealed record Disable2FARequest
{
    public required string Password { get; init; }
}
