using FAM.Application.Auth.Logout;
using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

public class LogoutAllDevicesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<ITokenBlacklistService> _mockTokenBlacklistService;
    private readonly Mock<ILogger<LogoutAllDevicesCommandHandler>> _mockLogger;
    private readonly LogoutAllDevicesCommandHandler _handler;

    public LogoutAllDevicesCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockTokenBlacklistService = new Mock<ITokenBlacklistService>();
        _mockLogger = new Mock<ILogger<LogoutAllDevicesCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        _handler = new LogoutAllDevicesCommandHandler(
            _mockUnitOfWork.Object,
            _mockTokenBlacklistService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldDeactivateAllDevices()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new LogoutAllDevicesCommand
        {
            UserId = user.Id
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(user.Id, null, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExceptDeviceId_ShouldDeactivateAllExceptSpecified()
    {
        // Arrange
        var plainPassword = "SecurePass123!";
        var user = User.Create(
            "testuser",
            "test@example.com",
            plainPassword
        );
        var exceptDeviceId = "current-device-123";

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(user.Id, exceptDeviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new LogoutAllDevicesCommand
        {
            UserId = user.Id,
            ExceptDeviceId = exceptDeviceId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(user.Id, exceptDeviceId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentUserId = 99999L;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LogoutAllDevicesCommand
        {
            UserId = nonExistentUserId
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}