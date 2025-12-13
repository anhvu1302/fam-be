using FAM.Application.Auth.Login;
using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;
using FAM.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ISigningKeyService> _mockSigningKeyService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IOtpService> _mockOtpService;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockSigningKeyService = new Mock<ISigningKeyService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockOtpService = new Mock<IOtpService>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        // Setup JWT service config properties
        _mockJwtService.Setup(x => x.AccessTokenExpiryMinutes).Returns(60);
        _mockJwtService.Setup(x => x.RefreshTokenExpiryDays).Returns(30);

        _handler = new LoginCommandHandler(
            _mockUnitOfWork.Object,
            _mockJwtService.Object,
            _mockSigningKeyService.Object,
            _mockEmailService.Object,
            _mockOtpService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentialsAndNo2FA_ShouldReturnTokens()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        // User.Create with plain password will hash it internally
        var user = User.Create("testuser", "test@example.com", plainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, 1L);
        user.VerifyEmail(); // Ensure email is verified

        var command = new LoginCommand
        {
            Identity = "testuser",
            Password = plainPassword, // Same plain password will verify correctly
            DeviceId = "device123",
            DeviceName = "Chrome Browser",
            DeviceType = "browser",
            RememberMe = false
        };

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        _mockUserDeviceRepository
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Mock signing key
        var mockSigningKey = SigningKey.Create(
            "test_key_id",
            "-----BEGIN PUBLIC KEY-----\ntest\n-----END PUBLIC KEY-----",
            "-----BEGIN RSA PRIVATE KEY-----\ntest\n-----END RSA PRIVATE KEY-----",
            "RS256");

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns("access_token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        LoginResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.RequiresTwoFactor.Should().BeFalse();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be("testuser");
        result.User.Email.Should().Be("test@example.com");

        _mockUserRepository.Verify(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Identity = "nonexistent",
            Password = "SecurePass123!",
            DeviceId = "device123"
        };

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username/email or password");

        _mockUserRepository.Verify(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var password = Password.Create("CorrectPass123!");
        var user = User.Create("testuser", "test@example.com", password.Hash, password.Salt, null, null);
        typeof(User).GetProperty("Id")?.SetValue(user, 1L);

        var command = new LoginCommand
        {
            Identity = "testuser",
            Password = "WrongPassword123!",
            DeviceId = "device123"
        };

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username/email or password");

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once); // Failed login recorded
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var password = Password.Create("SecurePass123!");
        var user = User.Create("testuser", "test@example.com", password.Hash, password.Salt, null, null);
        typeof(User).GetProperty("Id")?.SetValue(user, 1L);
        user.Deactivate(); // Deactivate user

        var command = new LoginCommand
        {
            Identity = "testuser",
            Password = "SecurePass123!",
            DeviceId = "device123"
        };

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is inactive");
    }

    [Fact]
    public async Task Handle_WithLockedOutUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var password = Password.Create("SecurePass123!");
        var user = User.Create("testuser", "test@example.com", password.Hash, password.Salt, null, null);
        typeof(User).GetProperty("Id")?.SetValue(user, 1L);

        // Lock user account
        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.RecordFailedLogin(); // 5 failed attempts -> locked

        var command = new LoginCommand
        {
            Identity = "testuser",
            Password = "SecurePass123!",
            DeviceId = "device123"
        };

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*locked*");
    }

    [Fact]
    public async Task Handle_With2FAEnabled_ShouldReturnSessionToken()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create("testuser", "test@example.com", plainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, 1L);
        user.VerifyEmail(); // Ensure email is verified
        user.EnableTwoFactor("secret", "{\"codes\":[\"code1\",\"code2\"]}");

        var command = new LoginCommand
        {
            Identity = "testuser",
            Password = plainPassword,
            DeviceId = "device123"
        };

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Mock signing key
        var mockSigningKey = SigningKey.Create(
            "test_key_id",
            "-----BEGIN PUBLIC KEY-----\ntest\n-----END PUBLIC KEY-----",
            "-----BEGIN RSA PRIVATE KEY-----\ntest\n-----END RSA PRIVATE KEY-----",
            "RS256");

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns("2fa_session_token");

        // Act
        LoginResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorSessionToken.Should().Be("2fa_session_token");
        result.AccessToken.Should().BeNullOrEmpty(); // Either null or empty string
        result.RefreshToken.Should().BeNullOrEmpty();
        result.User.IsTwoFactorEnabled.Should().BeTrue();

        _mockJwtService.Verify(x => x.GenerateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRememberMe_ShouldSetLongerRefreshTokenExpiry()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create("testuser", "test@example.com", plainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, 1L);
        user.VerifyEmail(); // Ensure email is verified

        var command = new LoginCommand
        {
            Identity = "testuser",
            Password = plainPassword,
            DeviceId = "device123",
            RememberMe = true // Remember me enabled
        };

        UserDevice? capturedDevice = null;

        _mockUserRepository
            .Setup(x => x.FindByIdentityAsync(command.Identity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        _mockUserDeviceRepository
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Callback<UserDevice, CancellationToken>((device, _) => capturedDevice = device)
            .Returns(Task.CompletedTask);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Mock signing key
        var mockSigningKey = SigningKey.Create(
            "test_key_id",
            "-----BEGIN PUBLIC KEY-----\ntest\n-----END PUBLIC KEY-----",
            "-----BEGIN RSA PRIVATE KEY-----\ntest\n-----END RSA PRIVATE KEY-----",
            "RS256");

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns("access_token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        LoginResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNull();
        capturedDevice.Should().NotBeNull();

        // With RememberMe, refresh token should expire in ~30 days
        DateTime expectedExpiry = DateTime.UtcNow.AddDays(30);
        capturedDevice!.RefreshTokenExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }
}
