using FAM.Domain.Abstractions.Storage;
using FAM.Domain.Common.Enums;
using FAM.Infrastructure.Common.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Minio.Exceptions;

namespace FAM.Infrastructure.Services;

/// <summary>
/// MinIO implementation of IStorageService with presigned URLs (time-limited access)
/// </summary>
public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(
        IMinioClient minioClient,
        IOptions<MinioSettings> settings,
        ILogger<MinioStorageService> logger)
    {
        _minioClient = minioClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        FileType fileType,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            string objectName = GenerateObjectName(fileName, fileType);

            PutObjectArgs? putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> InitiateMultipartUploadAsync(
        string fileName,
        FileType fileType,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            string objectName = GenerateObjectName(fileName, fileType);

            // MinIO SDK handles multipart internally, we create a unique upload ID
            string uploadId = $"{objectName}_{Guid.NewGuid():N}";

            return uploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating multipart upload for {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> UploadPartAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        int partNumber,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For simplicity, we'll store each part as a temporary object
            string tempObjectName = $"temp/{uploadId}/part-{partNumber}";

            PutObjectArgs? putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(tempObjectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length);

            PutObjectResponse? response = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            return response.Etag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading part {PartNumber} for uploadId {UploadId}",
                partNumber, uploadId);
            throw;
        }
    }

    public async Task<string> CompleteMultipartUploadAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        Dictionary<int, string> eTags,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string objectName = GenerateObjectName(fileName, fileType);

            // Use GetObjectAsync and PutObjectAsync to merge parts
            // This is a simplified approach - for production, consider using MinIO's actual multipart APIs
            using MemoryStream memoryStream = new();

            foreach (int partNumber in eTags.Keys.OrderBy(k => k))
            {
                string tempObjectName = $"temp/{uploadId}/part-{partNumber}";

                GetObjectArgs? getArgs = new GetObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(tempObjectName)
                    .WithCallbackStream(async (stream, ct) => { await stream.CopyToAsync(memoryStream, ct); });

                await _minioClient.GetObjectAsync(getArgs, cancellationToken);
            }

            // Upload merged file
            memoryStream.Position = 0;
            PutObjectArgs? putArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length);

            await _minioClient.PutObjectAsync(putArgs, cancellationToken);

            // Clean up temporary parts
            foreach (int partNumber in eTags.Keys)
            {
                string tempObjectName = $"temp/{uploadId}/part-{partNumber}";
                await DeleteFileAsync(tempObjectName, cancellationToken);
            }

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing multipart upload for uploadId {UploadId}", uploadId);
            throw;
        }
    }

    public async Task AbortMultipartUploadAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Clean up all temporary parts
            string prefix = $"temp/{uploadId}/";
            ListObjectsArgs? listArgs = new ListObjectsArgs()
                .WithBucket(_settings.BucketName)
                .WithPrefix(prefix)
                .WithRecursive(true);

            await foreach (Item? obj in _minioClient.ListObjectsEnumAsync(listArgs, cancellationToken))
            {
                await DeleteFileAsync(obj.Key, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting multipart upload for uploadId {UploadId}", uploadId);
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        string filePath,
        int expiryInSeconds = 3600)
    {
        try
        {
            PresignedGetObjectArgs? args = new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath)
                .WithExpiry(expiryInSeconds);

            string? url = await _minioClient.PresignedGetObjectAsync(args);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for {FilePath}", filePath);
            throw;
        }
    }

    public async Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        FileType fileType,
        string contentType,
        int expiryInSeconds = 3600)
    {
        try
        {
            await EnsureBucketExistsAsync();

            string objectName = GenerateObjectName(fileName, fileType);

            PresignedPutObjectArgs? args = new PresignedPutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithExpiry(expiryInSeconds);

            string? url = await _minioClient.PresignedPutObjectAsync(args);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned upload URL for {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            RemoveObjectArgs? args = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath);

            await _minioClient.RemoveObjectAsync(args, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            StatObjectArgs? args = new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath);

            await _minioClient.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<StorageFileInfo> GetFileInfoAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            StatObjectArgs? args = new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath);

            ObjectStat? stat = await _minioClient.StatObjectAsync(args, cancellationToken);

            return new StorageFileInfo
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                Size = stat.Size,
                ContentType = stat.ContentType,
                LastModified = stat.LastModified,
                ETag = stat.ETag
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FilePath}", filePath);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            BucketExistsArgs? bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(_settings.BucketName);

            bool exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (!exists)
            {
                MakeBucketArgs? makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_settings.BucketName);

                if (!string.IsNullOrEmpty(_settings.Region))
                {
                    makeBucketArgs.WithLocation(_settings.Region);
                }

                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket {Bucket} exists", _settings.BucketName);
            throw;
        }
    }

    private static string GenerateObjectName(string fileName, FileType fileType)
    {
        string extension = Path.GetExtension(fileName);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        string sanitizedFileName = SanitizeFileName(fileNameWithoutExt);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        string uniqueId = Guid.NewGuid().ToString("N")[..8];

        string folder = fileType switch
        {
            FileType.Image => "images",
            FileType.Media => "media",
            FileType.Document => "documents",
            _ => "others"
        };

        return $"{folder}/{timestamp}-{sanitizedFileName}-{uniqueId}{extension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }

    public async Task<string> GetPresignedPutUrlAsync(
        string objectKey,
        string contentType,
        int expiryInSeconds = 3600,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            PresignedPutObjectArgs? args = new PresignedPutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectKey)
                .WithExpiry(expiryInSeconds);

            string? url = await _minioClient.PresignedPutObjectAsync(args);

            _logger.LogDebug("Generated presigned PUT URL for {ObjectKey}, expires in {Seconds}s", objectKey,
                expiryInSeconds);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned PUT URL for {ObjectKey}", objectKey);
            throw;
        }
    }

    public async Task MoveObjectAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // MinIO doesn't have native "move", so we copy then delete
            await CopyObjectAsync(sourceKey, destKey, cancellationToken);
            await DeleteFileAsync(sourceKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving object from {Source} to {Dest}", sourceKey, destKey);
            throw;
        }
    }

    public async Task CopyObjectAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CopySourceObjectArgs? copySourceArgs = new CopySourceObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(sourceKey);

            CopyObjectArgs? copyObjectArgs = new CopyObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(destKey)
                .WithCopyObjectSource(copySourceArgs);

            await _minioClient.CopyObjectAsync(copyObjectArgs, cancellationToken);

            _logger.LogDebug("Copied object from {Source} to {Dest}", sourceKey, destKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying object from {Source} to {Dest}", sourceKey, destKey);
            throw;
        }
    }

    public async Task DeleteObjectsAsync(
        IEnumerable<string> objectKeys,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> keysList = objectKeys.ToList();
            if (!keysList.Any())
            {
                return;
            }

            // MinIO batch delete - returns list of errors
            RemoveObjectsArgs? removeObjectsArgs = new RemoveObjectsArgs()
                .WithBucket(_settings.BucketName)
                .WithObjects(keysList);

            IList<DeleteError>? errors = await _minioClient.RemoveObjectsAsync(removeObjectsArgs, cancellationToken);

            if (errors?.Any() == true)
            {
                _logger.LogWarning("Errors during batch delete: {ErrorCount}", errors.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple objects");
            throw;
        }
    }
}
