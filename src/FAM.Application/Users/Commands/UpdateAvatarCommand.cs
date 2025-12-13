using MediatR;

namespace FAM.Application.Users.Commands;

/// <summary>
/// Update user avatar using upload session pattern
/// Finalizes the uploaded file and links it to user
/// </summary>
public sealed record UpdateAvatarCommand : IRequest<UpdateAvatarResponse>
{
    public required long UserId { get; init; }
    public required string UploadId { get; init; }
}

public sealed record UpdateAvatarResponse
{
    public required string AvatarUrl { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
