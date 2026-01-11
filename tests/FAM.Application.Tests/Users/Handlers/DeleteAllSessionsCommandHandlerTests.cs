using System.Reflection;

using FAM.Application.Users.Commands.DeleteAllSessions;
using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;

using Microsoft.Extensions.Logging;

using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class DeleteAllSessionsCommandHandlerTests
{
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITokenBlacklistService> _mockTokenBlacklistService;
    private readonly Mock<ILogger<DeleteAllSessionsCommandHandler>> _mockLogger;
    private readonly DeleteAllSessionsCommandHandler _handler;

    public DeleteAllSessionsCommandHandlerTests()
    {
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTokenBlacklistService = new Mock<ITokenBlacklistService>();
        _mockLogger = new Mock<ILogger<DeleteAllSessionsCommandHandler>>();

        _handler = new DeleteAllSessionsCommandHandler(
            _mockUserDeviceRepository.Object,
            _mockUnitOfWork.Object,
            _mockTokenBlacklistService.Object);
    }

    [Fact]
    public async Task Handle_WithoutExcludedDevice_ShouldDeactivateAllSessions()
    {
        // Arrange
        long userId = 1L;
        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(userId, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DeleteAllSessionsCommand command = new(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(userId, null, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExcludedDevice_ShouldDeactivateAllExceptExcluded()
    {
        // Arrange
        long userId = 1L;
        string excludeDeviceId = "device123";

        // Create a real device using factory method
        UserDevice device = UserDevice.Create(
            userId,
            excludeDeviceId,
            "Test Device",
            "browser",
            "Test UA",
            "127.0.0.1",
            "Test Location"
        );

        // Set creation date to 4 days ago to make it eligible for trust
        PropertyInfo? createdAtField = device.GetType().GetProperty("CreatedAt",
            BindingFlags.Public | BindingFlags.Instance);
        createdAtField?.SetValue(device, DateTime.UtcNow.AddDays(-4));

        // Mark device as trusted
        PropertyInfo? isTrustedField = device.GetType().GetProperty("IsTrusted",
            BindingFlags.Public | BindingFlags.Instance);
        isTrustedField?.SetValue(device, true);

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(excludeDeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(userId, excludeDeviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockTokenBlacklistService
            .Setup(x => x.BlacklistUserTokensAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DeleteAllSessionsCommand command = new(userId, excludeDeviceId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserDeviceRepository.Verify(
            x => x.GetByDeviceIdAsync(excludeDeviceId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(userId, excludeDeviceId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
