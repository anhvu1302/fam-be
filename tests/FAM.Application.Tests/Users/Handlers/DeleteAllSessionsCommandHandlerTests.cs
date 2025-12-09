using FAM.Application.Users.Commands.DeleteAllSessions;
using FAM.Domain.Abstractions;
using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class DeleteAllSessionsCommandHandlerTests
{
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeleteAllSessionsCommandHandler _handler;

    public DeleteAllSessionsCommandHandlerTests()
    {
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new DeleteAllSessionsCommandHandler(_mockUserDeviceRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithoutExcludedDevice_ShouldDeactivateAllSessions()
    {
        // Arrange
        var userId = 1L;
        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(userId, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteAllSessionsCommand(userId);

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
        var userId = 1L;
        var excludeDeviceId = "device123";

        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(userId, excludeDeviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteAllSessionsCommand(userId, excludeDeviceId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(userId, excludeDeviceId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}