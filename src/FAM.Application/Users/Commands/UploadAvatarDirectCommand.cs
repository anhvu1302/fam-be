using MediatR;

namespace FAM.Application.Users.Commands;

/// <summary>
/// Upload user avatar directly (single-step upload with transaction safety)
/// Uploads file to storage, then updates user avatar atomically
/// If DB update fails, uploaded file is automatically cleaned up
/// </summary>
public sealed record UploadAvatarDirectCommand : IRequest<UploadAvatarDirectResponse>
{
    public required long UserId { get; init; }
    public required Stream FileStream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
}

public sealed record UploadAvatarDirectResponse
{
    public required string AvatarUrl { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string FilePath { get; init; }
}