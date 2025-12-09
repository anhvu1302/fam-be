using FAM.Application.Auth.Services;
using FAM.Application.Auth.VerifyEmailOtp;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

/// <summary>
/// Unit tests for VerifyEmailOtpLoginCommandHandler
/// Tests email OTP verification during login flow
/// </summary>
public class VerifyEmailOtpLoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOtpService> _mockOtpService;
    private readonly Mock<ISigningKeyService> _mockSigningKeyService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<VerifyEmailOtpLoginCommandHandler>> _mockLogger;
    private readonly VerifyEmailOtpLoginCommandHandler _handler;

    public VerifyEmailOtpLoginCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOtpService = new Mock<IOtpService>();
        _mockSigningKeyService = new Mock<ISigningKeyService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<VerifyEmailOtpLoginCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new VerifyEmailOtpLoginCommandHandler(
            _mockOtpService.Object,
            _mockUnitOfWork.Object,
            _mockSigningKeyService.Object,
            _mockJwtService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidOtpAndNo2FA_ShouldReturnAccessToken()
    {
        // Arrange
        const string email = "test@example.com";
        const string otp = "123456";
        const long userId = 1;

        var user = CreateTestUser(userId, email);

        _mockUserRepository.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOtpService.Setup(x => x.VerifyOtpAsync(userId, email, otp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockOtpService.Setup(x => x.RemoveOtpAsync(userId, email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var signingKey = SigningKey.Create("key1", "public_key", "private_key", "RS256", 2048);

        _mockSigningKeyService.Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(signingKey);

        _mockJwtService.Setup(x => x.GenerateAccessTokenWithRsa(
                userId, user.Username.Value, email, It.IsAny<List<string>>(),
                signingKey.KeyId, signingKey.PrivateKey, signingKey.Algorithm))
            .Returns("access_token");

        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        var command = new VerifyEmailOtpLoginCommand { Email = email, EmailOtp = otp };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.TokenType.Should().Be("Bearer");
        result.RequiresTwoFactor.Should().BeFalse();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(email);

        _mockOtpService.Verify(
            x => x.VerifyOtpAsync(userId, email, otp, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockOtpService.Verify(
            x => x.RemoveOtpAsync(userId, email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidOtpAndEmailNotVerified_ShouldMarkEmailAsVerified()
    {
        // Arrange
        const string email = "test@example.com";
        const string otp = "123456";
        const long userId = 1;

        var user = CreateTestUser(userId, email, false);

        _mockUserRepository.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOtpService.Setup(x => x.VerifyOtpAsync(userId, email, otp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockOtpService.Setup(x => x.RemoveOtpAsync(userId, email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var signingKey = SigningKey.Create("key1", "public_key", "private_key", "RS256", 2048);

        _mockSigningKeyService.Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(signingKey);

        _mockJwtService.Setup(x => x.GenerateAccessTokenWithRsa(
                userId, user.Username.Value, email, It.IsAny<List<string>>(),
                signingKey.KeyId, signingKey.PrivateKey, signingKey.Algorithm))
            .Returns("access_token");

        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        var command = new VerifyEmailOtpLoginCommand { Email = email, EmailOtp = otp };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");

        // Verify SaveChanges was called to persist email verification
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidOtpAnd2FAEnabled_ShouldReturnTwoFactorSessionToken()
    {
        // Arrange
        const string email = "test@example.com";
        const string otp = "123456";
        const long userId = 1;

        var user = CreateTestUser(userId, email, twoFactorEnabled: true);

        _mockUserRepository.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOtpService.Setup(x => x.VerifyOtpAsync(userId, email, otp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockOtpService.Setup(x => x.RemoveOtpAsync(userId, email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var signingKey = SigningKey.Create("key1", "public_key", "private_key", "RS256", 2048);

        _mockSigningKeyService.Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(signingKey);

        _mockJwtService.Setup(x => x.GenerateAccessTokenWithRsa(
                userId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                signingKey.KeyId, signingKey.PrivateKey, signingKey.Algorithm))
            .Returns("session_token");

        var command = new VerifyEmailOtpLoginCommand { Email = email, EmailOtp = otp };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorSessionToken.Should().Be("session_token");
        result.AccessToken.Should().BeNullOrEmpty();
        result.RefreshToken.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidOtp_ShouldThrowUnauthorizedException()
    {
        // Arrange
        const string email = "test@example.com";
        const string invalidOtp = "000000";
        const long userId = 1;

        var user = CreateTestUser(userId, email);

        _mockUserRepository.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOtpService.Setup(x => x.VerifyOtpAsync(userId, email, invalidOtp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new VerifyEmailOtpLoginCommand { Email = email, EmailOtp = invalidOtp };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

        _mockOtpService.Verify(
            x => x.RemoveOtpAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        const string email = "nonexistent@example.com";
        const string otp = "123456";

        _mockUserRepository.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new VerifyEmailOtpLoginCommand { Email = email, EmailOtp = otp };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

        _mockOtpService.Verify(
            x => x.VerifyOtpAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static User CreateTestUser(
        long id = 1,
        string email = "test@example.com",
        bool isEmailVerified = true,
        bool twoFactorEnabled = false)
    {
        var user = User.Create(
            $"testuser{id}",
            email,
            "Password123!",
            firstName: "Test",
            lastName: "User");

        // Use reflection to set IsEmailVerified if needed
        if (!isEmailVerified) typeof(User).GetProperty("IsEmailVerified")?.SetValue(user, false);

        // Use reflection to set TwoFactorEnabled if needed
        if (twoFactorEnabled) typeof(User).GetProperty("TwoFactorEnabled")?.SetValue(user, true);

        // Use reflection to set ID
        typeof(User).GetProperty("Id")?.SetValue(user, id);

        return user;
    }
}