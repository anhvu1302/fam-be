using FAM.Application.Auth.Enable2FA;
using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using FluentAssertions;

using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

public class Enable2FACommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Enable2FACommandHandler _handler;

    public Enable2FACommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new Enable2FACommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidPasswordAndUserId_ShouldReturnSecretAndQrCode()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new Enable2FACommand
        {
            UserId = user.Id,
            Password = plainPassword
        };

        // Act
        Enable2FAResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Secret.Should().NotBeNullOrEmpty();
        result.QrCodeUri.Should().NotBeNullOrEmpty();
        result.QrCodeUri.Should().Contain("otpauth://totp/FAM:");
        result.QrCodeUri.Should().Contain(Uri.EscapeDataString(user.Email)); // Email is URL encoded
        result.ManualEntryKey.Should().NotBeNullOrEmpty();
        result.ManualEntryKey.Should().Contain(" "); // Should be formatted with spaces
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var wrongPassword = "WrongPassword123!";
        var user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new Enable2FACommand
        {
            UserId = user.Id,
            Password = wrongPassword
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var nonExistentUserId = 99999L;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new Enable2FACommand
        {
            UserId = nonExistentUserId,
            Password = "SomePassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MultipleCalls_ShouldGenerateDifferentSecrets()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new Enable2FACommand
        {
            UserId = user.Id,
            Password = plainPassword
        };

        // Act
        Enable2FAResponse result1 = await _handler.Handle(command, CancellationToken.None);
        Enable2FAResponse result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert - Each call should generate a different secret
        result1.Secret.Should().NotBe(result2.Secret);
        result1.QrCodeUri.Should().NotBe(result2.QrCodeUri);
    }
}
