using FAM.Application.Auth.RefreshToken;
using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

using FluentAssertions;

using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ISigningKeyService> _mockSigningKeyService;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockSigningKeyService = new Mock<ISigningKeyService>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        // Setup JWT service config properties
        _mockJwtService.Setup(x => x.AccessTokenExpiryMinutes).Returns(60);
        _mockJwtService.Setup(x => x.RefreshTokenExpiryDays).Returns(30);

        _handler = new RefreshTokenCommandHandler(_mockUnitOfWork.Object, _mockJwtService.Object,
            _mockSigningKeyService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        UserDevice device = user.GetOrCreateDevice(
            "device-123",
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var oldRefreshToken = "old-refresh-token";
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        device.UpdateRefreshToken(oldRefreshToken, refreshTokenExpiry, "192.168.1.1");

        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(oldRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var mockSigningKey = SigningKey.Create("test-kid", "test-public-key", "test-private-key", "RS256");

        _mockSigningKeyService
            .Setup(x => x.GetOrCreateActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSigningKey);

        _mockJwtService
            .Setup(x => x.GenerateAccessTokenWithRsa(user.Id, user.Username.Value, user.Email.Value,
                It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(newAccessToken);

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);

        var command = new RefreshTokenCommand
        {
            RefreshToken = oldRefreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
        };

        // Act
        LoginResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(newAccessToken);
        result.RefreshToken.Should().Be(newRefreshToken);
        result.AccessTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
        result.RefreshTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
        result.TokenType.Should().Be("Bearer");
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be("testuser");

        _mockUserDeviceRepository.Verify(x => x.Update(It.IsAny<UserDevice>()), Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var invalidRefreshToken = "invalid-token";

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(invalidRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var command = new RefreshTokenCommand
        {
            RefreshToken = invalidRefreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        UserDevice device = user.GetOrCreateDevice(
            "device-123",
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var expiredRefreshToken = "expired-refresh-token";
        DateTime expiredDate = DateTime.UtcNow.AddDays(-1); // Expired yesterday
        device.UpdateRefreshToken(expiredRefreshToken, expiredDate, "192.168.1.1");

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(expiredRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var command = new RefreshTokenCommand
        {
            RefreshToken = expiredRefreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveDevice_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        UserDevice device = user.GetOrCreateDevice(
            "device-123",
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var refreshToken = "valid-refresh-token";
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiry, "192.168.1.1");
        device.Deactivate(); // Deactivate the device

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        user.Deactivate(); // Deactivate the user

        UserDevice device = user.GetOrCreateDevice(
            "device-123",
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var refreshToken = "valid-refresh-token";
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiry, "192.168.1.1");

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithLockedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        // Lock the user by recording failed attempts
        for (var i = 0; i < 5; i++) user.RecordFailedLogin();

        UserDevice device = user.GetOrCreateDevice(
            "device-123",
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var refreshToken = "valid-refresh-token";
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiry, "192.168.1.1");

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
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
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        UserDevice device = user.GetOrCreateDevice(
            "device-123",
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        var refreshToken = "valid-refresh-token";
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiry, "192.168.1.1");

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = "192.168.1.1",
            Location = "Hanoi, Vietnam"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
