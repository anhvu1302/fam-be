namespace FAM.Application.Settings;

/// <summary>
/// File upload configuration
/// </summary>
public class FileUploadSettings
{
    public const string SectionName = "FileUpload";

    /// <summary>
    /// Maximum image file size (MB)
    /// </summary>
    public int MaxImageSizeMb { get; set; } = 5;

    /// <summary>
    /// Maximum media file size (MB)
    /// </summary>
    public int MaxMediaSizeMb { get; set; } = 50;

    /// <summary>
    /// Maximum document file size (MB)
    /// </summary>
    public int MaxDocumentSizeMb { get; set; } = 10;

    /// <summary>
    /// Allowed extensions for images
    /// </summary>
    public string[] AllowedImageExtensions { get; set; } = 
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    /// <summary>
    /// Allowed extensions for media files
    /// </summary>
    public string[] AllowedMediaExtensions { get; set; } = 
    {
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm",
        ".mp3", ".wav", ".ogg", ".m4a"
    };

    /// <summary>
    /// Allowed extensions for documents
    /// </summary>
    public string[] AllowedDocumentExtensions { get; set; } = 
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv"
    };

    /// <summary>
    /// Chunk size for multipart upload (MB)
    /// </summary>
    public int MultipartChunkSizeMb { get; set; } = 5;

    /// <summary>
    /// Maximum number of parts for multipart upload
    /// </summary>
    public int MultipartMaxParts { get; set; } = 10000;
}
