using FAM.Domain.Common;

namespace FAM.Application.Common.Storage;

/// <summary>
/// Service for managing object storage (MinIO buckets)
/// </summary>
public interface IObjectStorageService
{
    /// <summary>
    /// Private bucket for sensitive/asset files (restricted access)
    /// </summary>
    string PrivateBucket => "fam-assets";

    /// <summary>
    /// Public bucket for shared files (read-only public access)
    /// </summary>
    string PublicBucket => "fam-public";

    /// <summary>
    /// Upload file to private bucket
    /// </summary>
    /// <param name="fileName">File name in bucket</param>
    /// <param name="fileStream">File stream</param>
    /// <param name="contentType">MIME type</param>
    /// <returns>Public URL to access the file</returns>
    Task<string> UploadPrivateAsync(string fileName, Stream fileStream, string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload file to public bucket
    /// </summary>
    /// <param name="fileName">File name in bucket</param>
    /// <param name="fileStream">File stream</param>
    /// <param name="contentType">MIME type</param>
    /// <returns>Public URL to access the file</returns>
    Task<string> UploadPublicAsync(string fileName, Stream fileStream, string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from private bucket
    /// </summary>
    Task DeletePrivateAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from public bucket
    /// </summary>
    Task DeletePublicAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get download URL for private file (with expiry)
    /// </summary>
    Task<string> GetPrivateDownloadUrlAsync(string fileName, TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get download URL for public file
    /// </summary>
    string GetPublicDownloadUrl(string fileName);
}
