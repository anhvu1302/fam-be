using FAM.Domain.Common;
using FAM.Domain.Common.Enums;

namespace FAM.Domain.Storage;

/// <summary>
/// Tracks temporary file uploads until they are finalized or expired
/// Implements "Upload tạm → Confirm khi create" pattern for eventual consistency
/// </summary>
public class UploadSession : Entity
{
    /// <summary>
    /// Unique identifier for this upload session (UUID)
    /// </summary>
    public string UploadId { get; private set; } = null!;

    /// <summary>
    /// Temporary storage key (e.g., "tmp/{upload_id}")
    /// </summary>
    public string TempKey { get; private set; } = null!;

    /// <summary>
    /// Original filename from client
    /// </summary>
    public string FileName { get; private set; } = null!;

    /// <summary>
    /// Detected file type
    /// </summary>
    public FileType FileType { get; private set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Content type (MIME)
    /// </summary>
    public string ContentType { get; private set; } = null!;

    /// <summary>
    /// Current status of upload session
    /// </summary>
    public UploadSessionStatus Status { get; private set; }

    /// <summary>
    /// When this session expires and can be cleaned up by GC
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Final storage key after finalization (null if not finalized)
    /// </summary>
    public string? FinalKey { get; private set; }

    /// <summary>
    /// Reference to entity that finalized this upload (e.g., asset_id)
    /// </summary>
    public int? EntityId { get; private set; }

    /// <summary>
    /// Type of entity (Asset, MaintenanceRecord, etc.)
    /// </summary>
    public string? EntityType { get; private set; }

    /// <summary>
    /// Checksum (MD5/SHA256) for integrity verification
    /// </summary>
    public string? Checksum { get; private set; }

    /// <summary>
    /// User who initiated the upload
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Idempotency key for safe retries
    /// </summary>
    public string? IdempotencyKey { get; private set; }

    // EF Core
    private UploadSession() { }

    /// <summary>
    /// Create a new upload session for temporary file storage
    /// </summary>
    public static UploadSession Create(
        string uploadId,
        string tempKey,
        string fileName,
        FileType fileType,
        long fileSize,
        string contentType,
        int userId,
        int ttlHours = 24,
        string? idempotencyKey = null)
    {
        var session = new UploadSession
        {
            UploadId = uploadId,
            TempKey = tempKey,
            FileName = fileName,
            FileType = fileType,
            FileSize = fileSize,
            ContentType = contentType,
            UserId = userId,
            Status = UploadSessionStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(ttlHours),
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Domain event can be added later if needed
        // session.AddDomainEvent(new UploadSessionCreated(...));
        return session;
    }

    /// <summary>
    /// Mark upload as completed (file successfully uploaded to temp location)
    /// </summary>
    public void MarkUploaded(string? checksum = null)
    {
        if (Status != UploadSessionStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark uploaded: session is {Status}");
        }

        Status = UploadSessionStatus.Uploaded;
        Checksum = checksum;
        UpdatedAt = DateTime.UtcNow;

        // AddDomainEvent(new UploadSessionUploaded(Id, UploadId, TempKey));
    }

    /// <summary>
    /// Finalize the upload: move to permanent storage and link to entity
    /// </summary>
    public void Finalize(string finalKey, int entityId, string entityType, string? checksum = null)
    {
        if (Status != UploadSessionStatus.Uploaded && Status != UploadSessionStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot finalize: session is {Status}");
        }

        // Verify checksum if provided
        if (!string.IsNullOrEmpty(checksum) && !string.IsNullOrEmpty(Checksum))
        {
            if (!checksum.Equals(Checksum, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Checksum mismatch");
            }
        }

        FinalKey = finalKey;
        EntityId = entityId;
        EntityType = entityType;
        Status = UploadSessionStatus.Finalized;
        UpdatedAt = DateTime.UtcNow;

        // AddDomainEvent(new UploadSessionFinalized(Id, UploadId, TempKey, finalKey, entityId, entityType));
    }

    /// <summary>
    /// Mark as failed (upload failed or validation failed)
    /// </summary>
    public void MarkFailed(string reason)
    {
        if (Status == UploadSessionStatus.Finalized || Status == UploadSessionStatus.CleanedUp)
        {
            throw new InvalidOperationException($"Cannot mark failed: session is {Status}");
        }

        Status = UploadSessionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        // AddDomainEvent(new UploadSessionFailed(Id, UploadId, TempKey, reason));
    }

    /// <summary>
    /// Mark as expired (TTL exceeded)
    /// </summary>
    public void MarkExpired()
    {
        if (Status == UploadSessionStatus.Finalized)
        {
            throw new InvalidOperationException("Cannot expire finalized session");
        }

        Status = UploadSessionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as cleaned up (temp file deleted by GC)
    /// </summary>
    public void MarkCleanedUp()
    {
        if (Status == UploadSessionStatus.Pending || Status == UploadSessionStatus.Uploaded)
        {
            Status = UploadSessionStatus.CleanedUp;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Check if session can be garbage collected
    /// </summary>
    public bool CanBeCleanedUp()
    {
        return (Status == UploadSessionStatus.Expired ||
                Status == UploadSessionStatus.Failed ||
                (Status == UploadSessionStatus.Pending && DateTime.UtcNow > ExpiresAt) ||
                (Status == UploadSessionStatus.Uploaded && DateTime.UtcNow > ExpiresAt));
    }
}

/// <summary>
/// Status of upload session lifecycle
/// </summary>
public enum UploadSessionStatus
{
    /// <summary>
    /// Session created, waiting for file upload
    /// </summary>
    Pending = 0,

    /// <summary>
    /// File uploaded to temp location, waiting for finalization
    /// </summary>
    Uploaded = 1,

    /// <summary>
    /// Successfully finalized and moved to permanent storage
    /// </summary>
    Finalized = 2,

    /// <summary>
    /// Upload or finalization failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Session expired (TTL exceeded)
    /// </summary>
    Expired = 4,

    /// <summary>
    /// Temp file cleaned up by garbage collector
    /// </summary>
    CleanedUp = 5
}
