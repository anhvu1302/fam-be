using FAM.Domain.Abstractions;
using FAM.Domain.Storage;

namespace FAM.Domain.Abstractions.Repositories;

/// <summary>
/// Repository for UploadSession aggregate
/// </summary>
public interface IUploadSessionRepository : IRepository<UploadSession, long>
{
    /// <summary>
    /// Find upload session by upload ID
    /// </summary>
    Task<UploadSession?> GetByUploadIdAsync(string uploadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find sessions that can be cleaned up (expired, failed, not finalized)
    /// </summary>
    Task<IReadOnlyList<UploadSession>> GetSessionsForCleanupAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find sessions by entity (for cleanup when entity is deleted)
    /// </summary>
    Task<IReadOnlyList<UploadSession>> GetByEntityAsync(
        int entityId,
        string entityType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find session by idempotency key (for idempotent retries)
    /// </summary>
    Task<UploadSession?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}