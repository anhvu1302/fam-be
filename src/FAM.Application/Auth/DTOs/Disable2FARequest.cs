namespace FAM.Application.Auth.DTOs;

public sealed record Disable2FARequest
{
    public required string Password { get; init; }
}