using System.Collections.Concurrent;
using FAM.Application.Abstractions;
using FAM.Domain.Common.Enums;

namespace FAM.IntegrationTests.Services;

/// <summary>
/// In-memory implementation of IStorageService for integration tests.
/// Stores files in memory to avoid dependency on external storage services like MinIO.
/// </summary>
public class InMemoryStorageService : IStorageService
{
    private readonly ConcurrentDictionary<string, StoredFile> _files = new();
    private readonly ConcurrentDictionary<string, MultipartUpload> _multipartUploads = new();

    private class StoredFile
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    private class MultipartUpload
    {
        public string FileName { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public ConcurrentDictionary<int, byte[]> Parts { get; set; } = new();
    }

    public Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        FileType fileType,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var objectName = GenerateObjectName(fileName, fileType);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        _files[objectName] = new StoredFile
        {
            Data = ms.ToArray(),
            ContentType = contentType,
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(objectName);
    }

    public Task<string> InitiateMultipartUploadAsync(
        string fileName,
        FileType fileType,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var objectName = GenerateObjectName(fileName, fileType);
        var uploadId = $"{objectName}_{Guid.NewGuid():N}";

        _multipartUploads[uploadId] = new MultipartUpload
        {
            FileName = fileName,
            FileType = fileType,
            ContentType = contentType
        };

        return Task.FromResult(uploadId);
    }

    public Task<string> UploadPartAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        int partNumber,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        if (!_multipartUploads.TryGetValue(uploadId, out var upload))
            throw new InvalidOperationException($"Upload {uploadId} not found");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        upload.Parts[partNumber] = ms.ToArray();

        var etag = $"etag-{partNumber}-{Guid.NewGuid():N}";
        return Task.FromResult(etag);
    }

    public Task<string> CompleteMultipartUploadAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        Dictionary<int, string> eTags,
        CancellationToken cancellationToken = default)
    {
        if (!_multipartUploads.TryGetValue(uploadId, out var upload))
            throw new InvalidOperationException($"Upload {uploadId} not found");

        // Merge all parts
        var sortedParts = upload.Parts.OrderBy(p => p.Key).SelectMany(p => p.Value).ToArray();
        var objectName = GenerateObjectName(fileName, fileType);

        _files[objectName] = new StoredFile
        {
            Data = sortedParts,
            ContentType = upload.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        _multipartUploads.TryRemove(uploadId, out _);

        return Task.FromResult(objectName);
    }

    public Task AbortMultipartUploadAsync(
        string uploadId,
        string fileName,
        FileType fileType,
        CancellationToken cancellationToken = default)
    {
        _multipartUploads.TryRemove(uploadId, out _);
        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(string filePath, int expiryInSeconds = 3600)
    {
        // Return a fake URL for testing - the actual URL doesn't matter in integration tests
        var url = $"http://localhost:9000/test-bucket/{filePath}?X-Amz-Expires={expiryInSeconds}&X-Amz-Signature=test";
        return Task.FromResult(url);
    }

    public Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        FileType fileType,
        string contentType,
        int expiryInSeconds = 3600)
    {
        var objectName = GenerateObjectName(fileName, fileType);
        var url =
            $"http://localhost:9000/test-bucket/{objectName}?X-Amz-Expires={expiryInSeconds}&X-Amz-Signature=test";
        return Task.FromResult(url);
    }

    public Task<string> GetPresignedPutUrlAsync(
        string objectKey,
        string contentType,
        int expiryInSeconds = 3600,
        CancellationToken cancellationToken = default)
    {
        var url = $"http://localhost:9000/test-bucket/{objectKey}?X-Amz-Expires={expiryInSeconds}&X-Amz-Signature=test";
        return Task.FromResult(url);
    }

    public Task MoveObjectAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default)
    {
        if (_files.TryRemove(sourceKey, out var file)) _files[destKey] = file;
        return Task.CompletedTask;
    }

    public Task CopyObjectAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default)
    {
        if (_files.TryGetValue(sourceKey, out var file))
            _files[destKey] = new StoredFile
            {
                Data = file.Data.ToArray(),
                ContentType = file.ContentType,
                CreatedAt = DateTime.UtcNow
            };
        return Task.CompletedTask;
    }

    public Task DeleteObjectsAsync(
        IEnumerable<string> objectKeys,
        CancellationToken cancellationToken = default)
    {
        foreach (var key in objectKeys) _files.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _files.TryRemove(filePath, out _);
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_files.ContainsKey(filePath));
    }

    public Task<Application.Abstractions.FileInfo> GetFileInfoAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!_files.TryGetValue(filePath, out var file)) throw new FileNotFoundException($"File {filePath} not found");

        return Task.FromResult(new Application.Abstractions.FileInfo
        {
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            Size = file.Data.Length,
            ContentType = file.ContentType,
            LastModified = file.CreatedAt,
            ETag = $"etag-{Guid.NewGuid():N}"
        });
    }

    private static string GenerateObjectName(string fileName, FileType fileType)
    {
        var folder = fileType switch
        {
            FileType.Image => "images",
            FileType.Document => "documents",
            FileType.Media => "media",
            _ => "others"
        };

        var uniqueId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(fileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");

        return $"{folder}/{timestamp}/{uniqueId}{extension}";
    }

    /// <summary>
    /// Clear all stored files - useful for test isolation
    /// </summary>
    public void Clear()
    {
        _files.Clear();
        _multipartUploads.Clear();
    }
}