using FAM.Application.Auth.Commands;
using FAM.Application.Auth.Handlers;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FluentAssertions;
using Moq;
using OtpNet;

namespace FAM.Application.Tests.Auth.Handlers;

public class Confirm2FACommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Confirm2FACommandHandler _handler;

    public Confirm2FACommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new Confirm2FACommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCode_ShouldEnable2FAAndReturnBackupCodes()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        // Generate a real secret and valid TOTP code
        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new Confirm2FACommand
        {
            UserId = user.Id,
            Secret = base32Secret,
            Code = validCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.BackupCodes.Should().NotBeNull();
        result.BackupCodes.Should().HaveCount(16); // Should have 16 backup codes
        result.BackupCodes.Should().OnlyContain(code => code.Length == 11); // Format: xxxxx-xxxxx
        result.BackupCodes.Should().OnlyContain(code => code.Contains("-")); // Should contain dash
        result.Message.Should().NotBeNullOrEmpty();

        // Verify user has 2FA enabled
        user.TwoFactorEnabled.Should().BeTrue();
        user.TwoFactorSecret.Should().Be(base32Secret);
        user.TwoFactorBackupCodes.Should().NotBeNullOrEmpty();

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCode_ShouldThrowInvalidOperationException()
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
        var invalidCode = "000000"; // Invalid code

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new Confirm2FACommand
        {
            UserId = user.Id,
            Secret = base32Secret,
            Code = invalidCode
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        // User should not have 2FA enabled
        user.TwoFactorEnabled.Should().BeFalse();
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var nonExistentUserId = 99999L;
        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new Confirm2FACommand
        {
            UserId = nonExistentUserId,
            Secret = base32Secret,
            Code = validCode
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BackupCodesShouldBeDifferent()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user1 = User.Create("user1", "user1@example.com", plainPassword);
        var user2 = User.Create("user2", "user2@example.com", plainPassword);

        var secretKey1 = KeyGeneration.GenerateRandomKey(32);
        var base32Secret1 = Base32Encoding.ToString(secretKey1);
        var totp1 = new Totp(secretKey1);
        var validCode1 = totp1.ComputeTotp();

        var secretKey2 = KeyGeneration.GenerateRandomKey(32);
        var base32Secret2 = Base32Encoding.ToString(secretKey2);
        var totp2 = new Totp(secretKey2);
        var validCode2 = totp2.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user1);

        var command1 = new Confirm2FACommand { UserId = user1.Id, Secret = base32Secret1, Code = validCode1 };
        var command2 = new Confirm2FACommand { UserId = user2.Id, Secret = base32Secret2, Code = validCode2 };

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user2);

        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert - Backup codes should be different
        result1.BackupCodes.Should().NotBeEquivalentTo(result2.BackupCodes);
    }

    [Fact]
    public async Task Handle_BackupCodesShouldFollowCorrectFormat()
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
        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new Confirm2FACommand
        {
            UserId = user.Id,
            Secret = base32Secret,
            Code = validCode
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Each backup code should match format: xxxxx-xxxxx
        foreach (var code in result.BackupCodes)
            code.Should().MatchRegex(@"^[0-9a-f]{5}-[0-9a-f]{5}$",
                $"code '{code}' should match format xxxxx-xxxxx with hex characters");
    }
}