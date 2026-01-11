using System.Reflection;

using FAM.Application.Users.Commands.DeleteSession;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users.Entities;

using FluentAssertions;

using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class DeleteSessionCommandHandlerTests
{
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITokenBlacklistService> _mockTokenBlacklistService;
    private readonly DeleteSessionCommandHandler _handler;

    public DeleteSessionCommandHandlerTests()
    {
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTokenBlacklistService = new Mock<ITokenBlacklistService>();
        _handler = new DeleteSessionCommandHandler(_mockUserDeviceRepository.Object, _mockUnitOfWork.Object,
            _mockTokenBlacklistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidSession_ShouldDeleteSession()
    {
        // Arrange
        long userId = 1L;
        Guid sessionId = Guid.NewGuid();
        string currentDeviceId = "currentDevice123";
        UserDevice device = UserDevice.Create(userId, "device1", "Chrome", "desktop");
        string accessToken = "test_token";
        DateTime expirationTime = DateTime.UtcNow.AddHours(1);

        // Mock current device as trusted (created 4 days ago)
        UserDevice currentDevice = UserDevice.Create(userId, currentDeviceId, "Chrome", "desktop");
        PropertyInfo? createdAtField = typeof(UserDevice).GetProperty("CreatedAt");
        createdAtField?.SetValue(currentDevice, DateTime.UtcNow.AddDays(-4));
        currentDevice.MarkAsTrusted();

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(currentDeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentDevice);

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockTokenBlacklistService
            .Setup(x => x.BlacklistTokenAsync(accessToken, expirationTime, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DeleteSessionCommand command = new(userId, sessionId, currentDeviceId, accessToken, expirationTime);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserDeviceRepository.Verify(x => x.Delete(device), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTokenBlacklistService.Verify(
            x => x.BlacklistTokenAsync(accessToken, expirationTime, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentSession_ShouldThrowDomainException()
    {
        // Arrange
        long userId = 1L;
        Guid sessionId = Guid.NewGuid();
        string currentDeviceId = "currentDevice123";

        // Mock current device as trusted
        UserDevice currentDevice = UserDevice.Create(userId, currentDeviceId, "Chrome", "desktop");
        PropertyInfo? createdAtField = typeof(UserDevice).GetProperty("CreatedAt");
        createdAtField?.SetValue(currentDevice, DateTime.UtcNow.AddDays(-4));
        currentDevice.MarkAsTrusted();

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(currentDeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentDevice);

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        DeleteSessionCommand command = new(userId, sessionId, currentDeviceId);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSessionFromDifferentUser_ShouldThrowDomainException()
    {
        // Arrange
        long userId = 1L;
        long otherUserId = 2L;
        Guid sessionId = Guid.NewGuid();
        string currentDeviceId = "currentDevice123";
        UserDevice device = UserDevice.Create(otherUserId, "device1", "Chrome", "desktop");

        // Mock current device as trusted
        UserDevice currentDevice = UserDevice.Create(userId, currentDeviceId, "Chrome", "desktop");
        PropertyInfo? createdAtField = typeof(UserDevice).GetProperty("CreatedAt");
        createdAtField?.SetValue(currentDevice, DateTime.UtcNow.AddDays(-4));
        currentDevice.MarkAsTrusted();

        _mockUserDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(currentDeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentDevice);

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        DeleteSessionCommand command = new(userId, sessionId, currentDeviceId);

        // Act & Assert
        DomainException exception =
            await Assert.ThrowsAsync<DomainException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

        exception.ErrorCode.Should().Be(ErrorCodes.USER_SESSION_NOT_FOUND);
    }
}
