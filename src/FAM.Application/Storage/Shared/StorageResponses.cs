namespace FAM.Application.Storage.Shared;

/// <summary>
/// Response after uploading a file
/// </summary>
public record UploadFileResponse(
    string FilePath,
    string Url,
    DateTime ExpiresAt,
    long FileSize
);

/// <summary>
/// Response after initiating multipart upload
/// </summary>
public record InitiateMultipartUploadResponse(
    string UploadId,
    string FilePath,
    long ChunkSize
);

/// <summary>
/// Response after uploading a part
/// </summary>
public record UploadPartResponse(
    int PartNumber,
    string ETag
);

/// <summary>
/// Response with presigned URL
/// </summary>
public record GetPresignedUrlResponse(
    string Url,
    DateTime ExpiresAt
);

/// <summary>
/// Response after finalization
/// </summary>
public record FinalizeUploadResponse(
    string FinalKey,
    string Url,
    DateTime ExpiresAt
);