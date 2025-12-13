namespace FAM.Application.Auth.Shared;

public sealed record Enable2FAResponse
{
    public required string Secret { get; init; }
    public required string QrCodeUri { get; init; }
    public required string ManualEntryKey { get; init; }
}
