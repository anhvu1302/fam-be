using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Auth.VerifyTwoFactor;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using OtpNet;

namespace FAM.Application.Tests.Auth.Handlers;

public class VerifyTwoFactorCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ISigningKeyService> _mockSigningKeyService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITwoFactorSessionService> _mockTwoFactorSessionService;
    private readonly Mock<ILogger<VerifyTwoFactorCommandHandler>> _mockLogger;
    private readonly VerifyTwoFactorCommandHandler _handler;

    public VerifyTwoFactorCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockSigningKeyService = new Mock<ISigningKeyService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<VerifyTwoFactorCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        // Setup JWT service config properties
        _mockJwtService.Setup(x => x.AccessTokenExpiryMinutes).Returns(60);
        _mockJwtService.Setup(x => x.RefreshTokenExpiryDays).Returns(30);

        _mockTwoFactorSessionService = new Mock<ITwoFactorSessionService>();
        // Setup 2FA session service to validate token
        _mockTwoFactorSessionService
            .Setup(x => x.ValidateAndGetUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1L); // Return user ID 1 for valid token
        _mockTwoFactorSessionService
            .Setup(x => x.RevokeSessionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new VerifyTwoFactorCommandHandler(
            _mockUserRepository.Object,
            _mockUserDeviceRepository.Object,
            _mockJwtService.Object,
            _mockSigningKeyService.Object,
            _mockTwoFactorSessionService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    private static void SetUserId(User user, long id)
    {
        // Use reflection to set Id (normally set by EF Core)
        typeof(User).GetProperty("Id")!.SetValue(user, id);
    }

    [Fact]
    public async Task Handle_WithValidCode_ShouldReturnTokensAndUserInfo()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        // Manually set Id since User.Create doesn't set it (normally set by EF Core)
        typeof(User).GetProperty("Id")!.SetValue(user, 1L);

        // Enable 2FA with a real secret
        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        // Generate valid TOTP code
        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var twoFactorSessionToken = "session-token-123";
        var accessToken = "access-token-123";
        var refreshToken = "refresh-token-123";
        var deviceId = "device-123";

        var mockSigningKey = SigningKey.Create("test-kid", "test-public-key", "test-private-key", "RS256");

        _mockJwtService
            .Setup(x => x.GetUserIdFromToken(twoFactorSessionToken))
            .Returns(user.Id);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(user.Id, user.Username.Value, user.Email.Value,
                It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(accessToken);

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null); // New device

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = twoFactorSessionToken,
            TwoFactorCode = validCode,
            DeviceId = deviceId,
            DeviceName = "Test Device",
            DeviceType = "browser",
            UserAgent = "Mozilla/5.0",
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam",
            RememberMe = false
        };

        // Act
        VerifyTwoFactorResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.AccessTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
        result.RefreshTokenExpiresAt.Should()
            .BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5)); // RememberMe = false
        result.TokenType.Should().Be("Bearer");
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be("testuser");
        result.User.IsTwoFactorEnabled.Should().BeTrue();

        _mockUserDeviceRepository.Verify(x => x.AddAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingDevice_ShouldUpdateDevice()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        SetUserId(user, 2L);

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var deviceId = "device-123";
        UserDevice existingDevice = user.GetOrCreateDevice(
            deviceId,
            "Existing Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var twoFactorSessionToken = "session-token-123";
        var accessToken = "access-token-123";
        var refreshToken = "refresh-token-123";

        var mockSigningKey = SigningKey.Create("test-kid", "test-public-key", "test-private-key", "RS256");

        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(user.Id);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(user.Id, user.Username.Value, user.Email.Value,
                It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(accessToken);
        _mockJwtService.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
        _mockUserDeviceRepository.Setup(x => x.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDevice);

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = twoFactorSessionToken,
            TwoFactorCode = validCode,
            DeviceId = deviceId,
            DeviceName = "Test Device",
            DeviceType = "browser",
            UserAgent = "Mozilla/5.0",
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam",
            RememberMe = false
        };

        // Act
        VerifyTwoFactorResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockUserDeviceRepository.Verify(x => x.Update(It.IsAny<UserDevice>()), Times.Once);
        _mockUserDeviceRepository.Verify(x => x.AddAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithRememberMe_ShouldSetLongerRefreshTokenExpiry()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        SetUserId(user, 3L);

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var twoFactorSessionToken = "session-token-123";
        var deviceId = "device-123";

        var mockSigningKey = SigningKey.Create("test-kid", "test-public-key", "test-private-key", "RS256");

        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(user.Id);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _mockJwtService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _mockUserDeviceRepository.Setup(x => x.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = twoFactorSessionToken,
            TwoFactorCode = validCode,
            DeviceId = deviceId,
            DeviceName = "Test Device",
            DeviceType = "browser",
            RememberMe = true // Remember me enabled
        };

        // Act
        VerifyTwoFactorResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // The refresh token should be set to expire in 30 days instead of 7
        UserDevice? addedDevice = user.UserDevices.FirstOrDefault(d => d.DeviceId == deviceId);
        addedDevice.Should().NotBeNull();
        addedDevice!.RefreshTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_WithInvalidCode_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        SetUserId(user, 4L);

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        var invalidCode = "000000"; // Invalid code
        var twoFactorSessionToken = "session-token-123";

        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(user.Id);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = twoFactorSessionToken,
            TwoFactorCode = invalidCode,
            DeviceId = "device-123",
            DeviceName = "Test Device",
            DeviceType = "browser"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidSessionToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var invalidSessionToken = "invalid-session-token";

        _mockJwtService
            .Setup(x => x.GetUserIdFromToken(invalidSessionToken))
            .Throws<UnauthorizedAccessException>();

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = invalidSessionToken,
            TwoFactorCode = "123456",
            DeviceId = "device-123",
            DeviceName = "Test Device",
            DeviceType = "browser"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUser2FANotEnabled_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        SetUserId(user, 5L);
        // User has no 2FA enabled

        var twoFactorSessionToken = "session-token-123";

        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(user.Id);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = twoFactorSessionToken,
            TwoFactorCode = "123456",
            DeviceId = "device-123",
            DeviceName = "Test Device",
            DeviceType = "browser"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var twoFactorSessionToken = "session-token-123";
        var nonExistentUserId = 99999L;

        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(nonExistentUserId);
        _mockUserRepository.Setup(x => x.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new VerifyTwoFactorCommand
        {
            TwoFactorSessionToken = twoFactorSessionToken,
            TwoFactorCode = "123456",
            DeviceId = "device-123",
            DeviceName = "Test Device",
            DeviceType = "browser"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
