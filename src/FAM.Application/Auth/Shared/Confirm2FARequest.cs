namespace FAM.Application.Auth.Shared;

public sealed record Confirm2FARequest
{
    public required string Secret { get; init; }
    public required string Code { get; init; }
}