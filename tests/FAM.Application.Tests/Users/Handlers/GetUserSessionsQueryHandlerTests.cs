using FAM.Application.Users.Queries.GetUserSessions;
using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;

using FluentAssertions;

using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class GetUserSessionsQueryHandlerTests
{
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly GetUserSessionsQueryHandler _handler;

    public GetUserSessionsQueryHandlerTests()
    {
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();
        _handler = new GetUserSessionsQueryHandler(_mockUserDeviceRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingSessions_ShouldReturnSessionsList()
    {
        // Arrange
        var userId = 1L;
        var devices = new List<UserDevice>
        {
            UserDevice.Create(userId, "device1", "Chrome on Windows", "desktop",
                "Mozilla/5.0...", "192.168.1.1", "New York, US", "Chrome", "Windows 11"),
            UserDevice.Create(userId, "device2", "Safari on iPhone", "mobile",
                "Mozilla/5.0...", "192.168.1.2", "New York, US", "Safari", "iOS 17")
        };

        _mockUserDeviceRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        var query = new GetUserSessionsQuery(userId);

        // Act
        IReadOnlyList<UserSessionDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(s => s.LastLoginAt);
    }

    [Fact]
    public async Task Handle_WithNoSessions_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 1L;
        _mockUserDeviceRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDevice>());

        var query = new GetUserSessionsQuery(userId);

        // Act
        IReadOnlyList<UserSessionDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
