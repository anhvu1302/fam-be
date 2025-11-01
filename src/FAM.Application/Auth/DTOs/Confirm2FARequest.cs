namespace FAM.Application.Auth.DTOs;

public sealed record Confirm2FARequest
{
    public required string Secret { get; init; }
    public required string Code { get; init; }
}
