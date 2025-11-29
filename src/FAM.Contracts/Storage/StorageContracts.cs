using FAM.Domain.Common.Enums;

namespace FAM.Contracts.Storage;

/// <summary>
/// Response after uploading a file
/// </summary>
public record UploadFileResponse
{
    /// <summary>
    /// File path in storage
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Presigned URL to access the file (time-limited)
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// URL expiration time
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; init; }
}

/// <summary>
/// Request to initiate multipart upload
/// </summary>
public record InitiateMultipartUploadRequest
{
    /// <summary>
    /// File name (file type will be auto-detected from extension)
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Content type
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Total file size in bytes
    /// </summary>
    public long TotalSize { get; init; }
}

/// <summary>
/// Response after initiating multipart upload
/// </summary>
public record InitiateMultipartUploadResponse
{
    /// <summary>
    /// Upload ID to use for uploading parts
    /// </summary>
    public string UploadId { get; init; } = string.Empty;

    /// <summary>
    /// File path where the file will be stored
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Recommended chunk size in bytes
    /// </summary>
    public long ChunkSize { get; init; }
}

/// <summary>
/// Request to upload a part in multipart upload
/// </summary>
public record UploadPartRequest
{
    /// <summary>
    /// Upload ID from InitiateMultipartUpload
    /// </summary>
    public string UploadId { get; init; } = string.Empty;

    /// <summary>
    /// Part number (starting from 1)
    /// </summary>
    public int PartNumber { get; init; }

    /// <summary>
    /// File name (to detect file type)
    /// </summary>
    public string FileName { get; init; } = string.Empty;
}

/// <summary>
/// Response after uploading a part
/// </summary>
public record UploadPartResponse
{
    /// <summary>
    /// Part number
    /// </summary>
    public int PartNumber { get; init; }

    /// <summary>
    /// ETag of the uploaded part
    /// </summary>
    public string ETag { get; init; } = string.Empty;
}

/// <summary>
/// Request to complete multipart upload
/// </summary>
public record CompleteMultipartUploadRequest
{
    /// <summary>
    /// Upload ID
    /// </summary>
    public string UploadId { get; init; } = string.Empty;

    /// <summary>
    /// File name (to detect file type)
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// List of ETags for each part (key: part number, value: ETag)
    /// </summary>
    public Dictionary<int, string> Parts { get; init; } = new();
}

/// <summary>
/// Request to get presigned URL
/// </summary>
public record GetPresignedUrlRequest
{
    /// <summary>
    /// File path in storage
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Expiration time in seconds (default: 1 hour)
    /// </summary>
    public int ExpiryInSeconds { get; init; } = 3600;
}

/// <summary>
/// Response with presigned URL
/// </summary>
public record GetPresignedUrlResponse
{
    /// <summary>
    /// Presigned URL (time-limited)
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// URL expiration time
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}