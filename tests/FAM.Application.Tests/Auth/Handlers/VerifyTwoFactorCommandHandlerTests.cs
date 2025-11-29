using FAM.Application.Auth.Commands;
using FAM.Application.Auth.Handlers;
using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OtpNet;
using Xunit;

namespace FAM.Application.Tests.Auth.Handlers;

public class VerifyTwoFactorCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<VerifyTwoFactorCommandHandler>> _mockLogger;
    private readonly VerifyTwoFactorCommandHandler _handler;

    public VerifyTwoFactorCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<VerifyTwoFactorCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        _handler = new VerifyTwoFactorCommandHandler(
            _mockUserRepository.Object,
            _mockUserDeviceRepository.Object,
            _mockJwtService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
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

        _mockJwtService
            .Setup(x => x.ValidateToken(twoFactorSessionToken))
            .Returns(true);

        _mockJwtService
            .Setup(x => x.GetUserIdFromToken(twoFactorSessionToken))
            .Returns(user.Id);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(user.Id, user.Username.Value, user.Email.Value, It.IsAny<List<string>>()))
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.ExpiresIn.Should().Be(3600);
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

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var deviceId = "device-123";
        var existingDevice = user.GetOrCreateDevice(
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

        _mockJwtService.Setup(x => x.ValidateToken(twoFactorSessionToken)).Returns(true);
        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(user.Id);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockJwtService
            .Setup(x => x.GenerateAccessToken(user.Id, user.Username.Value, user.Email.Value, It.IsAny<List<string>>()))
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
        var result = await _handler.Handle(command, CancellationToken.None);

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

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var twoFactorSessionToken = "session-token-123";
        var deviceId = "device-123";

        _mockJwtService.Setup(x => x.ValidateToken(twoFactorSessionToken)).Returns(true);
        _mockJwtService.Setup(x => x.GetUserIdFromToken(twoFactorSessionToken)).Returns(user.Id);
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<string>>())).Returns("access-token");
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // The refresh token should be set to expire in 30 days instead of 7
        var addedDevice = user.UserDevices.FirstOrDefault(d => d.DeviceId == deviceId);
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

        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.EnableTwoFactor(base32Secret, "[\"code1\",\"code2\"]");

        var invalidCode = "000000"; // Invalid code
        var twoFactorSessionToken = "session-token-123";

        _mockJwtService.Setup(x => x.ValidateToken(twoFactorSessionToken)).Returns(true);
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
            .Setup(x => x.ValidateToken(invalidSessionToken))
            .Returns(false);

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
        // User has no 2FA enabled

        var twoFactorSessionToken = "session-token-123";

        _mockJwtService.Setup(x => x.ValidateToken(twoFactorSessionToken)).Returns(true);
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

        _mockJwtService.Setup(x => x.ValidateToken(twoFactorSessionToken)).Returns(true);
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