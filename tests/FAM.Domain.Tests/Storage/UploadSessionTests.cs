using FAM.Domain.Common.Enums;
using FAM.Domain.Storage;

using FluentAssertions;

namespace FAM.Domain.Tests.Storage;

public class UploadSessionTests
{
    [Fact]
    public void Create_ShouldCreateUploadSessionWithCorrectProperties()
    {
        // Arrange
        string uploadId = Guid.NewGuid().ToString("N");
        string tempKey = $"tmp/{uploadId}";
        string fileName = "test-file.pdf";
        FileType fileType = FileType.Document;
        long fileSize = 1024000L;
        string contentType = "application/pdf";
        int userId = 123;
        int ttlHours = 24;
        string idempotencyKey = "test-idempotency-key";

        // Act
        UploadSession session = UploadSession.Create(
            uploadId,
            tempKey,
            fileName,
            fileType,
            fileSize,
            contentType,
            userId,
            ttlHours,
            idempotencyKey);

        // Assert
        session.Should().NotBeNull();
        session.UploadId.Should().Be(uploadId);
        session.TempKey.Should().Be(tempKey);
        session.FileName.Should().Be(fileName);
        session.FileType.Should().Be(fileType);
        session.FileSize.Should().Be(fileSize);
        session.ContentType.Should().Be(contentType);
        session.UserId.Should().Be(userId);
        session.IdempotencyKey.Should().Be(idempotencyKey);
        session.Status.Should().Be(UploadSessionStatus.Pending);
        session.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(ttlHours), TimeSpan.FromSeconds(5));
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkUploaded_ShouldUpdateStatusToUploaded()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        string checksum = "abc123def456";

        // Act
        session.MarkUploaded(checksum);

        // Assert
        session.Status.Should().Be(UploadSessionStatus.Uploaded);
        session.Checksum.Should().Be(checksum);
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkUploaded_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkUploaded();

        // Act
        Action act = () => session.MarkUploaded();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*session is Uploaded*");
    }

    [Fact]
    public void Finalize_ShouldUpdateStatusToFinalized()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkUploaded("checksum123");
        string finalKey = "permanent/file-key";
        int entityId = 456;
        string entityType = "Asset";

        // Act
        session.Finalize(finalKey, entityId, entityType);

        // Assert
        session.Status.Should().Be(UploadSessionStatus.Finalized);
        session.FinalKey.Should().Be(finalKey);
        session.EntityId.Should().Be(entityId);
        session.EntityType.Should().Be(entityType);
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Finalize_WithChecksumMismatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkUploaded("checksum123");
        string finalKey = "permanent/file-key";
        int entityId = 456;
        string entityType = "Asset";
        string wrongChecksum = "wrongchecksum";

        // Act
        Action act = () => session.Finalize(finalKey, entityId, entityType, wrongChecksum);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Checksum mismatch");
    }

    [Fact]
    public void Finalize_WhenNotUploadedOrPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkFailed("Test failure");

        // Act
        Action act = () => session.Finalize("key", 1, "Asset");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*session is Failed*");
    }

    [Fact]
    public void MarkFailed_ShouldUpdateStatusToFailed()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        string reason = "Upload failed due to network error";

        // Act
        session.MarkFailed(reason);

        // Assert
        session.Status.Should().Be(UploadSessionStatus.Failed);
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkFailed_WhenFinalized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkUploaded();
        session.Finalize("key", 1, "Asset");

        // Act
        Action act = () => session.MarkFailed("reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*session is Finalized*");
    }

    [Fact]
    public void MarkExpired_ShouldUpdateStatusToExpired()
    {
        // Arrange
        UploadSession session = CreateTestSession();

        // Act
        session.MarkExpired();

        // Assert
        session.Status.Should().Be(UploadSessionStatus.Expired);
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkExpired_WhenFinalized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkUploaded();
        session.Finalize("key", 1, "Asset");

        // Act
        Action act = () => session.MarkExpired();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot expire finalized session");
    }

    [Fact]
    public void MarkCleanedUp_ShouldUpdateStatusToCleanedUp()
    {
        // Arrange
        UploadSession session = CreateTestSession();

        // Act
        session.MarkCleanedUp();

        // Assert
        session.Status.Should().Be(UploadSessionStatus.CleanedUp);
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkCleanedUp_WhenFinalized_ShouldNotUpdateStatus()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.MarkUploaded();
        session.Finalize("key", 1, "Asset");
        UploadSessionStatus originalStatus = session.Status;

        // Act
        session.MarkCleanedUp();

        // Assert
        session.Status.Should().Be(originalStatus);
    }

    [Theory]
    [InlineData(UploadSessionStatus.Expired, true)]
    [InlineData(UploadSessionStatus.Failed, true)]
    [InlineData(UploadSessionStatus.Finalized, false)]
    public void CanBeCleanedUp_ShouldReturnCorrectValue(UploadSessionStatus status, bool expectedCanCleanup)
    {
        // Arrange
        UploadSession session = CreateTestSession();

        // Set status based on test case
        switch (status)
        {
            case UploadSessionStatus.Expired:
                session.MarkExpired();
                break;
            case UploadSessionStatus.Failed:
                session.MarkFailed("test");
                break;
            case UploadSessionStatus.Finalized:
                session.MarkUploaded();
                session.Finalize("key", 1, "Asset");
                break;
        }

        // Act
        bool canCleanup = session.CanBeCleanedUp();

        // Assert
        canCleanup.Should().Be(expectedCanCleanup);
    }

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        long deletedById = 999L;

        // Act
        session.SoftDelete(deletedById);

        // Assert
        session.IsDeleted.Should().BeTrue();
        session.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Restore_ShouldUnmarkAsDeleted()
    {
        // Arrange
        UploadSession session = CreateTestSession();
        session.SoftDelete();

        // Act
        session.Restore();

        // Assert
        session.IsDeleted.Should().BeFalse();
        session.DeletedAt.Should().BeNull();
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private static UploadSession CreateTestSession()
    {
        string uploadId = Guid.NewGuid().ToString("N");
        return UploadSession.Create(
            uploadId,
            $"tmp/{uploadId}",
            "test-file.pdf",
            FileType.Document,
            1024000L,
            "application/pdf",
            123,
            24);
    }
}
