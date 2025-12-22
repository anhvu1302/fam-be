using FAM.Application.Abstractions;
using FAM.Application.Storage.Commands;
using FAM.Application.Storage.Handlers;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Domain.Common.Enums;
using FAM.Domain.Storage;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FAM.Application.Tests.Storage;

public class InitUploadSessionHandlerTests
{
    private readonly Mock<IUploadSessionRepository> _mockSessionRepository;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<IFileValidator> _mockFileValidator;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<InitUploadSessionHandler>> _mockLogger;
    private readonly InitUploadSessionHandler _handler;

    public InitUploadSessionHandlerTests()
    {
        _mockSessionRepository = new Mock<IUploadSessionRepository>();
        _mockStorageService = new Mock<IStorageService>();
        _mockFileValidator = new Mock<IFileValidator>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<InitUploadSessionHandler>>();

        _handler = new InitUploadSessionHandler(
            _mockSessionRepository.Object,
            _mockStorageService.Object,
            _mockFileValidator.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateSessionAndReturnResponse()
    {
        // Arrange
        var command = new InitUploadSessionCommand
        {
            FileName = "test-document.pdf",
            ContentType = "application/pdf",
            FileSize = 1024000L,
            UserId = 123L,
            IdempotencyKey = null
        };

        _mockFileValidator
            .Setup(x => x.ValidateFile(command.FileName, command.FileSize))
            .Returns((true, null, FileType.Document));

        _mockStorageService
            .Setup(x => x.GetPresignedPutUrlAsync(
                It.IsAny<string>(),
                command.ContentType,
                3600,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://presigned-url.example.com/upload");

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UploadId.Should().NotBeNullOrEmpty();
        result.TempKey.Should().StartWith("tmp/");
        result.PresignedPutUrl.Should().Be("https://presigned-url.example.com/upload");
        result.FileType.Should().Be(FileType.Document);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));

        _mockSessionRepository.Verify(x => x.AddAsync(
            It.Is<UploadSession>(s => 
                s.FileName == command.FileName &&
                s.FileSize == command.FileSize &&
                s.ContentType == command.ContentType &&
                s.UserId == (int)command.UserId &&
                s.Status == UploadSessionStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIdempotencyKey_WhenSessionExists_ShouldReturnExistingSession()
    {
        // Arrange
        var idempotencyKey = "test-idempotency-key";
        var existingSession = UploadSession.Create(
            Guid.NewGuid().ToString("N"),
            "tmp/existing",
            "existing.pdf",
            FileType.Document,
            1024000L,
            "application/pdf",
            123,
            24,
            idempotencyKey);

        var command = new InitUploadSessionCommand
        {
            FileName = "test-document.pdf",
            ContentType = "application/pdf",
            FileSize = 1024000L,
            UserId = 123L,
            IdempotencyKey = idempotencyKey
        };

        _mockSessionRepository
            .Setup(x => x.GetByIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSession);

        _mockStorageService
            .Setup(x => x.GetPresignedPutUrlAsync(
                existingSession.TempKey,
                existingSession.ContentType,
                3600,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://presigned-url.example.com/upload");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UploadId.Should().Be(existingSession.UploadId);
        result.TempKey.Should().Be(existingSession.TempKey);

        _mockSessionRepository.Verify(x => x.AddAsync(
            It.IsAny<UploadSession>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidFile_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new InitUploadSessionCommand
        {
            FileName = "malicious.exe",
            ContentType = "application/x-msdownload",
            FileSize = 1024000L,
            UserId = 123L
        };

        _mockFileValidator
            .Setup(x => x.ValidateFile(command.FileName, command.FileSize))
            .Returns((false, "File type not allowed", (FileType?)null));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("File type not allowed");

        _mockSessionRepository.Verify(x => x.AddAsync(
            It.IsAny<UploadSession>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithOversizedFile_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new InitUploadSessionCommand
        {
            FileName = "large-file.pdf",
            ContentType = "application/pdf",
            FileSize = 200_000_000L, // 200 MB
            UserId = 123L
        };

        _mockFileValidator
            .Setup(x => x.ValidateFile(command.FileName, command.FileSize))
            .Returns((false, "File size exceeds maximum allowed (100 MB)", (FileType?)null));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("File size exceeds maximum allowed (100 MB)");
    }

    [Theory]
    [InlineData("document.pdf", "application/pdf", FileType.Document)]
    [InlineData("image.jpg", "image/jpeg", FileType.Image)]
    [InlineData("video.mp4", "video/mp4", FileType.Media)]
    public async Task Handle_WithDifferentFileTypes_ShouldCreateCorrectSession(
        string fileName,
        string contentType,
        FileType expectedFileType)
    {
        // Arrange
        var command = new InitUploadSessionCommand
        {
            FileName = fileName,
            ContentType = contentType,
            FileSize = 1024000L,
            UserId = 123L
        };

        _mockFileValidator
            .Setup(x => x.ValidateFile(command.FileName, command.FileSize))
            .Returns((true, null, expectedFileType));

        _mockStorageService
            .Setup(x => x.GetPresignedPutUrlAsync(
                It.IsAny<string>(),
                contentType,
                3600,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://presigned-url.example.com/upload");

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileType.Should().Be(expectedFileType);

        _mockSessionRepository.Verify(x => x.AddAsync(
            It.Is<UploadSession>(s => s.FileType == expectedFileType),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueUploadIdAndTempKey()
    {
        // Arrange
        var command = new InitUploadSessionCommand
        {
            FileName = "test.pdf",
            ContentType = "application/pdf",
            FileSize = 1024000L,
            UserId = 123L
        };

        _mockFileValidator
            .Setup(x => x.ValidateFile(command.FileName, command.FileSize))
            .Returns((true, null, FileType.Document));

        _mockStorageService
            .Setup(x => x.GetPresignedPutUrlAsync(
                It.IsAny<string>(),
                command.ContentType,
                3600,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://presigned-url.example.com/upload");

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.UploadId.Should().NotBe(result2.UploadId);
        result1.TempKey.Should().NotBe(result2.TempKey);
        result1.TempKey.Should().Contain(result1.UploadId);
        result2.TempKey.Should().Contain(result2.UploadId);
    }
}
