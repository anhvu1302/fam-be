using FAM.Domain.Common.Enums;
using MediatR;

namespace FAM.Application.Storage.Commands;

/// <summary>
/// Initialize upload session to get presigned URL for temporary upload
/// Implements "Upload tạm" step of the pattern
/// </summary>
public sealed record InitUploadSessionCommand : IRequest<InitUploadSessionResponse>
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required long UserId { get; init; } // From authenticated user
    public string? IdempotencyKey { get; init; }
}

public sealed record InitUploadSessionResponse
{
    public required string UploadId { get; init; }
    public required string TempKey { get; init; }
    public required string PresignedPutUrl { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required FileType FileType { get; init; }
}