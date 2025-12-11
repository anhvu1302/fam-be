using FAM.Application.Auth.Services;
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
        _handler = new DeleteSessionCommandHandler(_mockUserDeviceRepository.Object, _mockUnitOfWork.Object, _mockTokenBlacklistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidSession_ShouldDeleteSession()
    {
        // Arrange
        var userId = 1L;
        var sessionId = Guid.NewGuid();
        var device = UserDevice.Create(userId, "device1", "Chrome", "desktop");
        var accessToken = "test_token";
        var expirationTime = DateTime.UtcNow.AddHours(1);

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockTokenBlacklistService
            .Setup(x => x.BlacklistTokenAsync(accessToken, expirationTime, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteSessionCommand(userId, sessionId, accessToken, expirationTime);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserDeviceRepository.Verify(x => x.Delete(device), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTokenBlacklistService.Verify(x => x.BlacklistTokenAsync(accessToken, expirationTime, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentSession_ShouldThrowDomainException()
    {
        // Arrange
        var userId = 1L;
        var sessionId = Guid.NewGuid();

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var command = new DeleteSessionCommand(userId, sessionId);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSessionFromDifferentUser_ShouldThrowDomainException()
    {
        // Arrange
        var userId = 1L;
        var otherUserId = 2L;
        var sessionId = Guid.NewGuid();
        var device = UserDevice.Create(otherUserId, "device1", "Chrome", "desktop");

        _mockUserDeviceRepository
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var command = new DeleteSessionCommand(userId, sessionId);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<DomainException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

        exception.ErrorCode.Should().Be(ErrorCodes.USER_SESSION_NOT_FOUND);
    }
}