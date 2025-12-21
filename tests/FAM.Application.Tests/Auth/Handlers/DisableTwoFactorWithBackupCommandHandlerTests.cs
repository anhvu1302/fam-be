using System.Text.Json;

using FAM.Application.Auth.DisableTwoFactorWithBackup;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using OtpNet;

namespace FAM.Application.Tests.Auth.Handlers;

public class DisableTwoFactorWithBackupCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<DisableTwoFactorWithBackupCommandHandler>> _mockLogger;
    private readonly DisableTwoFactorWithBackupCommandHandler _handler;

    public DisableTwoFactorWithBackupCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<DisableTwoFactorWithBackupCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new DisableTwoFactorWithBackupCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidBackupCode_ShouldDisable2FA()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var backupCode = "12345-67890";
        var hashedBackupCode = BCrypt.Net.BCrypt.HashPassword(backupCode);

        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var backupCodesJson = JsonSerializer.Serialize(new List<string> { hashedBackupCode });
        user.EnableTwoFactor(base32Secret, backupCodesJson);

        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "testuser",
            Password = plainPassword,
            BackupCode = backupCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidBackupCode_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var correctBackupCode = "12345-67890";
        var wrongBackupCode = "99999-88888";
        var hashedBackupCode = BCrypt.Net.BCrypt.HashPassword(correctBackupCode);

        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var backupCodesJson = JsonSerializer.Serialize(new List<string> { hashedBackupCode });
        user.EnableTwoFactor(base32Secret, backupCodesJson);

        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "testuser",
            Password = plainPassword,
            BackupCode = wrongBackupCode
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));

        user.TwoFactorEnabled.Should().BeTrue(); // Should still be enabled
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var wrongPassword = "WrongPassword123!";
        var backupCode = "12345-67890";
        var hashedBackupCode = BCrypt.Net.BCrypt.HashPassword(backupCode);

        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var backupCodesJson = JsonSerializer.Serialize(new List<string> { hashedBackupCode });
        user.EnableTwoFactor(base32Secret, backupCodesJson);

        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "testuser",
            Password = wrongPassword,
            BackupCode = backupCode
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));

        user.TwoFactorEnabled.Should().BeTrue();
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "nonexistent",
            Password = "SomePassword123!",
            BackupCode = "12345-67890"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUser2FANotEnabled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        // User has no 2FA enabled

        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "testuser",
            Password = plainPassword,
            BackupCode = "12345-67890"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleBackupCodes_ShouldRemoveOnlyUsedCode()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var backupCode1 = "11111-22222";
        var backupCode2 = "33333-44444";
        var backupCode3 = "55555-66666";

        var hashedCode1 = BCrypt.Net.BCrypt.HashPassword(backupCode1);
        var hashedCode2 = BCrypt.Net.BCrypt.HashPassword(backupCode2);
        var hashedCode3 = BCrypt.Net.BCrypt.HashPassword(backupCode3);

        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var backupCodesJson =
            JsonSerializer.Serialize(new List<string> { hashedCode1, hashedCode2, hashedCode3 });
        user.EnableTwoFactor(base32Secret, backupCodesJson);

        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "testuser",
            Password = plainPassword,
            BackupCode = backupCode2 // Use the second backup code
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeFalse(); // Handler disables 2FA completely

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyBackupCodes_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[]"); // Empty backup codes array

        _mockUserRepository
            .Setup(x => x.FindByUsernameAsync("testuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DisableTwoFactorWithBackupCommand
        {
            Username = "testuser",
            Password = plainPassword,
            BackupCode = "12345-67890"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
