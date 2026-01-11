using FAM.Domain.Common.Enums;

namespace FAM.Domain.Abstractions.Storage;

/// <summary>
/// Service validate file upload
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Detect file type from file name extension
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>FileType or null if not supported</returns>
    FileType? DetectFileType(string fileName);

    /// <summary>
    /// Validate file (check extension and size)
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="fileSize">File size (bytes)</param>
    /// <returns>Tuple (isValid, errorMessage, detectedFileType)</returns>
    (bool IsValid, string? ErrorMessage, FileType? FileType) ValidateFile(
        string fileName,
        long fileSize);

    /// <summary>
    /// Get maximum file size for file type (bytes)
    /// </summary>
    long GetMaxFileSize(FileType fileType);

    /// <summary>
    /// Get allowed extensions for file type
    /// </summary>
    string[] GetAllowedExtensions(FileType fileType);

    /// <summary>
    /// Check if extension is allowed
    /// </summary>
    bool IsExtensionAllowed(string fileName, FileType fileType);

    /// <summary>
    /// Check if size is allowed
    /// </summary>
    bool IsSizeAllowed(long fileSize, FileType fileType);
}
