using FAM.Application.Auth.Logout;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;

using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<ITokenBlacklistService> _mockTokenBlacklistService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<LogoutCommandHandler>> _mockLogger;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockTokenBlacklistService = new Mock<ITokenBlacklistService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<LogoutCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        _handler = new LogoutCommandHandler(
            _mockUnitOfWork.Object,
            _mockTokenBlacklistService.Object,
            _mockJwtService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldDeactivateDevice()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

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

        var command = new LogoutCommand
        {
            RefreshToken = refreshToken,
            IpAddress = "192.168.1.1"
        };

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        device.IsActive.Should().BeFalse();
        device.RefreshToken.Should().BeNull();
        device.RefreshTokenExpiresAt.Should().BeNull();

        _mockUserDeviceRepository.Verify(x => x.Update(It.IsAny<UserDevice>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidDeviceId_ShouldDeactivateDevice()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.CreateWithPlainPassword(
            "testuser", "test@example.com", plainPassword);

        var deviceId = "device-123";
        UserDevice device = user.GetOrCreateDevice(
            deviceId,
            "Test Device",
            "browser",
            "Mozilla/5.0",
            "192.168.1.1",
            "Hanoi, Vietnam"
        );

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LogoutCommand
        {
            DeviceId = deviceId,
            IpAddress = "192.168.1.1"
        };

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        device.IsActive.Should().BeFalse();

        _mockUserDeviceRepository.Verify(x => x.Update(It.IsAny<UserDevice>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnSuccessWithoutError()
    {
        // Arrange
        var invalidRefreshToken = "invalid-token";

        _mockUserDeviceRepository
            .Setup(x => x.FindByRefreshTokenAsync(invalidRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var command = new LogoutCommand
        {
            RefreshToken = invalidRefreshToken,
            IpAddress = "192.168.1.1"
        };

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should not throw, just return success
        result.Should().Be(Unit.Value);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidDeviceId_ShouldReturnSuccessWithoutError()
    {
        // Arrange
        var invalidDeviceId = "invalid-device";

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(invalidDeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var command = new LogoutCommand
        {
            DeviceId = invalidDeviceId,
            IpAddress = "192.168.1.1"
        };

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should not throw, just return success
        result.Should().Be(Unit.Value);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoTokenOrDeviceId_ShouldReturnSuccessWithoutError()
    {
        // Arrange
        var command = new LogoutCommand
        {
            IpAddress = "192.168.1.1"
        };

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should not throw, just return success
        result.Should().Be(Unit.Value);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
