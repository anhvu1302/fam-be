using FAM.Application.Auth.Confirm2FA;
using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
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
        string plainPassword = "SecurePass123!";
        User user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        // Generate a real secret and valid TOTP code
        byte[]? secretKey = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret = Base32Encoding.ToString(secretKey);

        // Set pending 2FA secret (simulating Enable2FA call)
        user.SetPendingTwoFactorSecret(base32Secret, 10);

        Totp totp = new(secretKey);
        string? validCode = totp.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Confirm2FACommand command = new()
        {
            UserId = user.Id,
            Secret = base32Secret,
            Code = validCode
        };

        // Act
        Confirm2FAResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BackupCodes.Should().NotBeNull();
        result.BackupCodes.Should().HaveCount(16); // Should have 16 backup codes
        result.BackupCodes.Should().OnlyContain(code => code.Length == 11); // Format: xxxxx-xxxxx
        result.BackupCodes.Should().OnlyContain(code => code.Contains("-")); // Should contain dash

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
        string plainPassword = "SecurePass123!";
        User user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        byte[]? secretKey = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret = Base32Encoding.ToString(secretKey);

        // Set pending 2FA secret (simulating Enable2FA call)
        user.SetPendingTwoFactorSecret(base32Secret, 10);

        string invalidCode = "000000"; // Invalid code

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Confirm2FACommand command = new()
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
        long nonExistentUserId = 99999L;
        byte[]? secretKey = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret = Base32Encoding.ToString(secretKey);
        Totp totp = new(secretKey);
        string? validCode = totp.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Confirm2FACommand command = new()
        {
            UserId = nonExistentUserId,
            Secret = base32Secret,
            Code = validCode
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BackupCodesShouldBeDifferent()
    {
        // Arrange
        string plainPassword = "SecurePass123!";
        User user1 = User.CreateWithPlainPassword("user1", "user1@example.com", plainPassword);
        User user2 = User.CreateWithPlainPassword("user2", "user2@example.com", plainPassword);

        byte[]? secretKey1 = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret1 = Base32Encoding.ToString(secretKey1);

        // Set pending 2FA secret (simulating Enable2FA call)
        user1.SetPendingTwoFactorSecret(base32Secret1, 10);

        Totp totp1 = new(secretKey1);
        string? validCode1 = totp1.ComputeTotp();

        byte[]? secretKey2 = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret2 = Base32Encoding.ToString(secretKey2);

        // Set pending 2FA secret for user2 (simulating Enable2FA call)
        user2.SetPendingTwoFactorSecret(base32Secret2, 10);

        Totp totp2 = new(secretKey2);
        string? validCode2 = totp2.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user1);

        Confirm2FACommand command1 = new() { UserId = user1.Id, Secret = base32Secret1, Code = validCode1 };
        Confirm2FACommand command2 = new() { UserId = user2.Id, Secret = base32Secret2, Code = validCode2 };

        // Act
        Confirm2FAResponse result1 = await _handler.Handle(command1, CancellationToken.None);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user2);

        Confirm2FAResponse result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert - Backup codes should be different
        result1.BackupCodes.Should().NotBeEquivalentTo(result2.BackupCodes);
    }

    [Fact]
    public async Task Handle_BackupCodesShouldFollowCorrectFormat()
    {
        // Arrange
        string plainPassword = "SecurePass123!";
        User user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        byte[]? secretKey = KeyGeneration.GenerateRandomKey(32);
        string? base32Secret = Base32Encoding.ToString(secretKey);

        // Set pending 2FA secret (simulating Enable2FA call)
        user.SetPendingTwoFactorSecret(base32Secret, 10);

        Totp totp = new(secretKey);
        string? validCode = totp.ComputeTotp();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Confirm2FACommand command = new()
        {
            UserId = user.Id,
            Secret = base32Secret,
            Code = validCode
        };

        // Act
        Confirm2FAResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Each backup code should match format: xxxxx-xxxxx
        foreach (string code in result.BackupCodes)
        {
            code.Should().MatchRegex(@"^[0-9a-f]{5}-[0-9a-f]{5}$",
                $"code '{code}' should match format xxxxx-xxxxx with hex characters");
        }
    }
}
