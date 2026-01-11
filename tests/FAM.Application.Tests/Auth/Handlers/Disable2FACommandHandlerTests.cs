using FAM.Application.Auth.Disable2FA;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using FluentAssertions;

using Moq;

using OtpNet;

namespace FAM.Application.Tests.Auth.Handlers;

public class Disable2FACommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Disable2FACommandHandler _handler;

    public Disable2FACommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new Disable2FACommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidPassword_ShouldDisable2FA()
    {
        // Arrange
        string plainPassword = "SecurePass123!";
        User user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        // Enable 2FA first
        byte[]? secretKey = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Disable2FACommand command = new()
        {
            UserId = user.Id,
            Password = plainPassword
        };

        // Act
        bool result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();
        user.TwoFactorBackupCodes.Should().BeNull();

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        string plainPassword = "SecurePass123!";
        string wrongPassword = "WrongPassword123!";
        User user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        byte[]? secretKey = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Disable2FACommand command = new()
        {
            UserId = user.Id,
            Password = wrongPassword
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));

        // 2FA should still be enabled
        user.TwoFactorEnabled.Should().BeTrue();
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        long nonExistentUserId = 99999L;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Disable2FACommand command = new()
        {
            UserId = nonExistentUserId,
            Password = "SomePassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserHasNo2FAEnabled_ShouldStillSucceed()
    {
        // Arrange
        string plainPassword = "SecurePass123!";
        User user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);
        // User has no 2FA enabled

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Disable2FACommand command = new()
        {
            UserId = user.Id,
            Password = plainPassword
        };

        // Act
        bool result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should still succeed (idempotent operation)
        result.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeFalse();

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
