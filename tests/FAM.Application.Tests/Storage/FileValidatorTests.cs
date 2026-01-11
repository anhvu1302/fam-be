using FAM.Application.Settings;
using FAM.Application.Storage;
using FAM.Domain.Common.Enums;

using Microsoft.Extensions.Options;

namespace FAM.Application.Tests.Storage;

public class FileValidatorTests
{
    private readonly FileValidator _validator;
    private readonly FileUploadSettings _settings;

    public FileValidatorTests()
    {
        _settings = new FileUploadSettings
        {
            MaxImageSizeMb = 5,
            MaxMediaSizeMb = 50,
            MaxDocumentSizeMb = 10,
            AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" },
            AllowedMediaExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".mp3", ".wav" },
            AllowedDocumentExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" }
        };

        IOptions<FileUploadSettings> options = Options.Create(_settings);
        _validator = new FileValidator(options);
    }

    [Theory]
    [InlineData("image.jpg", FileType.Image)]
    [InlineData("photo.PNG", FileType.Image)]
    [InlineData("animation.gif", FileType.Image)]
    [InlineData("video.mp4", FileType.Media)]
    [InlineData("song.MP3", FileType.Media)]
    [InlineData("movie.avi", FileType.Media)]
    [InlineData("document.pdf", FileType.Document)]
    [InlineData("report.docx", FileType.Document)]
    [InlineData("spreadsheet.xlsx", FileType.Document)]
    public void DetectFileType_ValidExtension_ReturnsCorrectType(string fileName, FileType expectedType)
    {
        // Act
        FileType? result = _validator.DetectFileType(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Value);
    }

    [Theory]
    [InlineData("malware.exe")]
    [InlineData("script.js")]
    [InlineData("app.apk")]
    [InlineData("archive.zip")]
    [InlineData("noextension")]
    public void DetectFileType_InvalidExtension_ReturnsNull(string fileName)
    {
        // Act
        FileType? result = _validator.DetectFileType(fileName);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("image.jpg", 1024 * 1024)] // 1 MB
    [InlineData("image.jpg", 5 * 1024 * 1024)] // 5 MB (max)
    public void ValidateFile_ValidImageSize_ReturnsSuccess(string fileName, long fileSize)
    {
        // Act
        (bool isValid, string? errorMessage, FileType? fileType) = _validator.ValidateFile(fileName, fileSize);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
        Assert.Equal(FileType.Image, fileType);
    }

    [Fact]
    public void ValidateFile_ImageTooLarge_ReturnsError()
    {
        // Arrange
        string fileName = "large-image.jpg";
        int fileSize = 6 * 1024 * 1024; // 6 MB

        // Act
        (bool isValid, string? errorMessage, FileType? fileType) = _validator.ValidateFile(fileName, fileSize);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains("too large", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("5.00 MB", errorMessage);
        Assert.Equal(FileType.Image, fileType);
    }

    [Fact]
    public void ValidateFile_UnsupportedExtension_ReturnsError()
    {
        // Arrange
        string fileName = "malware.exe";
        int fileSize = 1024;

        // Act
        (bool isValid, string? errorMessage, FileType? fileType) = _validator.ValidateFile(fileName, fileSize);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains("not supported", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(".exe", errorMessage);
        Assert.Null(fileType);
    }

    [Fact]
    public void ValidateFile_EmptyFile_ReturnsError()
    {
        // Arrange
        string fileName = "empty.jpg";
        int fileSize = 0;

        // Act
        (bool isValid, string? errorMessage, FileType? fileType) = _validator.ValidateFile(fileName, fileSize);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains("empty", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateFile_NegativeSize_ReturnsError()
    {
        // Arrange
        string fileName = "test.jpg";
        int fileSize = -100;

        // Act
        (bool isValid, string? errorMessage, FileType? fileType) = _validator.ValidateFile(fileName, fileSize);

        // Assert
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
    }

    [Theory]
    [InlineData(FileType.Image, 5 * 1024 * 1024)]
    [InlineData(FileType.Media, 50 * 1024 * 1024)]
    [InlineData(FileType.Document, 10 * 1024 * 1024)]
    public void GetMaxFileSize_ReturnsCorrectSize(FileType fileType, long expectedSize)
    {
        // Act
        long result = _validator.GetMaxFileSize(fileType);

        // Assert
        Assert.Equal(expectedSize, result);
    }

    [Theory]
    [InlineData("test.jpg", FileType.Image, true)]
    [InlineData("test.JPG", FileType.Image, true)]
    [InlineData("test.png", FileType.Image, true)]
    [InlineData("test.pdf", FileType.Image, false)]
    [InlineData("test.mp4", FileType.Media, true)]
    [InlineData("test.jpg", FileType.Media, false)]
    public void IsExtensionAllowed_ChecksCorrectly(string fileName, FileType fileType, bool expected)
    {
        // Act
        bool result = _validator.IsExtensionAllowed(fileName, fileType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FileType.Image, 1024, true)]
    [InlineData(FileType.Image, 5 * 1024 * 1024, true)]
    [InlineData(FileType.Image, 6 * 1024 * 1024, false)]
    [InlineData(FileType.Media, 50 * 1024 * 1024, true)]
    [InlineData(FileType.Media, 51 * 1024 * 1024, false)]
    public void IsSizeAllowed_ChecksCorrectly(FileType fileType, long fileSize, bool expected)
    {
        // Act
        bool result = _validator.IsSizeAllowed(fileSize, fileType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAllowedExtensions_Image_ReturnsCorrectExtensions()
    {
        // Act
        string[] extensions = _validator.GetAllowedExtensions(FileType.Image);

        // Assert
        Assert.NotEmpty(extensions);
        Assert.Contains(".jpg", extensions);
        Assert.Contains(".png", extensions);
        Assert.Contains(".gif", extensions);
    }

    [Fact]
    public void GetAllowedExtensions_Media_ReturnsCorrectExtensions()
    {
        // Act
        string[] extensions = _validator.GetAllowedExtensions(FileType.Media);

        // Assert
        Assert.NotEmpty(extensions);
        Assert.Contains(".mp4", extensions);
        Assert.Contains(".mp3", extensions);
    }

    [Fact]
    public void GetAllowedExtensions_Document_ReturnsCorrectExtensions()
    {
        // Act
        string[] extensions = _validator.GetAllowedExtensions(FileType.Document);

        // Assert
        Assert.NotEmpty(extensions);
        Assert.Contains(".pdf", extensions);
        Assert.Contains(".docx", extensions);
    }
}
