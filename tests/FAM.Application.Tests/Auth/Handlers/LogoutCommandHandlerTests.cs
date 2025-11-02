using FAM.Application.Auth.Commands;
using FAM.Application.Auth.Handlers;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.Users.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace FAM.Application.Tests.Auth.Handlers;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        _handler = new LogoutCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldDeactivateDevice()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            username: "testuser",
            email: "test@example.com",
            plainPassword: plainPassword
        );
        
        var device = user.GetOrCreateDevice(
            deviceId: "device-123",
            deviceName: "Test Device",
            deviceType: "browser",
            userAgent: "Mozilla/5.0",
            ipAddress: "192.168.1.1",
            location: "Hanoi, Vietnam"
        );
        
        var refreshToken = "valid-refresh-token";
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
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
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var user = User.Create(
            username: "testuser",
            email: "test@example.com",
            plainPassword: plainPassword
        );
        
        var deviceId = "device-123";
        var device = user.GetOrCreateDevice(
            deviceId: deviceId,
            deviceName: "Test Device",
            deviceType: "browser",
            userAgent: "Mozilla/5.0",
            ipAddress: "192.168.1.1",
            location: "Hanoi, Vietnam"
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
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should not throw, just return success
        result.Should().Be(Unit.Value);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
