using FAM.Domain.Common.Enums;

namespace FAM.Application.Abstractions;

/// <summary>
/// Service quản lý storage (upload, download, delete files)
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Upload file thông thường
    /// </summary>
    /// <param name="stream">Stream của file</param>
    /// <param name="fileName">Tên file</param>
    /// <param name="fileType">Loại file</param>
    /// <param name="contentType">Content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Đường dẫn file đã upload</returns>
    Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        FileType fileType,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Khởi tạo multipart upload
    /// </summary>
    /// <param name="fileName">Tên file</param>
    /// <param name="fileType">Loại file</param>
    /// <param name="contentType">Content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload ID để sử dụng cho các part tiếp theo</returns>
    Task<string> InitiateMultipartUploadAsync(
        string fileName,
        FileType fileType,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload một part của multipart upload
    /// </summary>
    /// <param name="uploadId">Upload ID từ InitiateMultipartUploadAsync</param>
    /// <param name="fileName">Tên file</param>
    /// <param name="fileType">Loại file</param>
    /// <param name="partNumber">Số thứ tự của part (bắt đầu từ 1)</param>
    /// <param name="stream">Stream của part</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ETag của part đã upload</returns>
    Task<string> UploadPartAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        int partNumber,
        Stream stream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hoàn tất multipart upload
    /// </summary>
    /// <param name="uploadId">Upload ID</param>
    /// <param name="fileName">Tên file</param>
    /// <param name="fileType">Loại file</param>
    /// <param name="eTags">Danh sách ETags của các part theo thứ tự</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Đường dẫn file đã upload</returns>
    Task<string> CompleteMultipartUploadAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        Dictionary<int, string> eTags,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy multipart upload
    /// </summary>
    Task AbortMultipartUploadAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo presigned URL để download file (có thời gian hết hạn)
    /// </summary>
    /// <param name="filePath">Đường dẫn file</param>
    /// <param name="expiryInSeconds">Thời gian hết hạn (giây), mặc định 1 giờ</param>
    /// <returns>URL có thời gian hết hạn</returns>
    Task<string> GetPresignedUrlAsync(
        string filePath,
        int expiryInSeconds = 3600);

    /// <summary>
    /// Tạo presigned URL để upload file (có thời gian hết hạn)
    /// </summary>
    /// <param name="fileName">Tên file</param>
    /// <param name="fileType">Loại file</param>
    /// <param name="contentType">Content type</param>
    /// <param name="expiryInSeconds">Thời gian hết hạn (giây), mặc định 1 giờ</param>
    /// <returns>URL có thời gian hết hạn để upload</returns>
    Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        FileType fileType,
        string contentType,
        int expiryInSeconds = 3600);

    /// <summary>
    /// Tạo presigned PUT URL để upload trực tiếp (dùng cho upload session pattern)
    /// </summary>
    /// <param name="objectKey">Storage key (e.g., "tmp/uuid")</param>
    /// <param name="contentType">Content type</param>
    /// <param name="expiryInSeconds">Thời gian hết hạn (giây)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Presigned PUT URL</returns>
    Task<string> GetPresignedPutUrlAsync(
        string objectKey,
        string contentType,
        int expiryInSeconds = 3600,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Move/Copy object từ key này sang key khác (dùng cho finalization)
    /// </summary>
    Task MoveObjectAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Copy object từ key này sang key khác
    /// </summary>
    Task CopyObjectAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa nhiều files cùng lúc (batch delete cho GC)
    /// </summary>
    Task DeleteObjectsAsync(
        IEnumerable<string> objectKeys,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa file
    /// </summary>
    Task DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra file có tồn tại không
    /// </summary>
    Task<bool> FileExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin file
    /// </summary>
    Task<FileInfo> GetFileInfoAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Thông tin file
/// </summary>
public class FileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;
}
