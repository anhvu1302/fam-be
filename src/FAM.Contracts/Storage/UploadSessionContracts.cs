namespace FAM.Contracts.Storage;

/// <summary>
/// Request to initialize upload session
/// </summary>
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
public class FinalizeUploadRequest
{
    public required string UploadId { get; set; }
    public required string EntityType { get; set; } // "User", "Asset", etc.
    public string? Checksum { get; set; }
}

/// <summary>
/// Response after finalization
/// </summary>
public class FinalizeUploadResponse
{
    public required string FinalKey { get; set; }
    public required string Url { get; set; }
    public required DateTime ExpiresAt { get; set; }
}