using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.Storage;

/// <summary>
/// Request to initiate multipart upload
/// </summary>
[SwaggerSchema(Required = new[] { "fileName", "contentType", "totalSize" })]
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
/// Request to upload a part in multipart upload
/// </summary>
[SwaggerSchema(Required = new[] { "uploadId", "partNumber", "fileName" })]
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
/// Request to complete multipart upload
/// </summary>
[SwaggerSchema(Required = new[] { "uploadId", "fileName", "parts" })]
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
[SwaggerSchema(Required = new[] { "filePath" })]
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
/// Request to initialize upload session
/// </summary>
[SwaggerSchema(Required = new[] { "fileName", "contentType", "fileSize" })]
public class InitUploadSessionRequest
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required long FileSize { get; set; }
    public string? IdempotencyKey { get; set; }
}

/// <summary>
/// Request to finalize upload and link to entity
/// </summary>
[SwaggerSchema(Required = new[] { "uploadId", "entityType" })]
public class FinalizeUploadRequest
{
    public required string UploadId { get; set; }
    public required string EntityType { get; set; } // "User", "Asset", etc.
    public string? Checksum { get; set; }
}
