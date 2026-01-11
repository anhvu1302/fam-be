using FAM.Application.Settings;
using FAM.Domain.Abstractions.Storage;
using FAM.Domain.Common.Enums;

using Microsoft.Extensions.Options;

namespace FAM.Application.Storage;

/// <summary>
/// Implementation of IFileValidator
/// </summary>
public class FileValidator : IFileValidator
{
    private readonly FileUploadSettings _settings;

    public FileValidator(IOptions<FileUploadSettings> settings)
    {
        _settings = settings.Value;
    }

    public FileType? DetectFileType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (_settings.AllowedImageExtensions.Contains(extension))
        {
            return FileType.Image;
        }

        if (_settings.AllowedMediaExtensions.Contains(extension))
        {
            return FileType.Media;
        }

        if (_settings.AllowedDocumentExtensions.Contains(extension))
        {
            return FileType.Document;
        }

        return null;
    }

    public (bool IsValid, string? ErrorMessage, FileType? FileType) ValidateFile(
        string fileName,
        long fileSize)
    {
        // Check empty/invalid size first
        if (fileSize <= 0)
        {
            return (false, "File is empty or invalid", null);
        }

        // Detect file type from extension
        FileType? fileType = DetectFileType(fileName);

        if (!fileType.HasValue)
        {
            string extension = Path.GetExtension(fileName);
            return (false,
                $"File extension '{extension}' is not supported. Allowed: images (.jpg, .png, etc.), media (.mp4, .mp3, etc.), documents (.pdf, .doc, etc.)",
                null);
        }

        // Check size
        if (!IsSizeAllowed(fileSize, fileType.Value))
        {
            double maxSizeMb = GetMaxFileSize(fileType.Value) / (1024.0 * 1024.0);
            return (false, $"File is too large. Maximum size for {fileType.Value}: {maxSizeMb:F2} MB", fileType);
        }

        return (true, null, fileType);
    }

    public long GetMaxFileSize(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => _settings.MaxImageSizeMb * 1024L * 1024L,
            FileType.Media => _settings.MaxMediaSizeMb * 1024L * 1024L,
            FileType.Document => _settings.MaxDocumentSizeMb * 1024L * 1024L,
            _ => throw new ArgumentException($"Unknown file type: {fileType}")
        };
    }

    public string[] GetAllowedExtensions(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => _settings.AllowedImageExtensions,
            FileType.Media => _settings.AllowedMediaExtensions,
            FileType.Document => _settings.AllowedDocumentExtensions,
            _ => throw new ArgumentException($"Unknown file type: {fileType}")
        };
    }

    public bool IsExtensionAllowed(string fileName, FileType fileType)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        string[] allowedExtensions = GetAllowedExtensions(fileType);
        return allowedExtensions.Contains(extension);
    }

    public bool IsSizeAllowed(long fileSize, FileType fileType)
    {
        long maxSize = GetMaxFileSize(fileType);
        return fileSize > 0 && fileSize <= maxSize;
    }
}
