using FAM.Application.Users.Queries.GetUserTheme;
using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;
using FluentAssertions;
using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class GetUserThemeQueryHandlerTests
{
    private readonly Mock<IUserThemeRepository> _mockUserThemeRepository;
    private readonly GetUserThemeQueryHandler _handler;

    public GetUserThemeQueryHandlerTests()
    {
        _mockUserThemeRepository = new Mock<IUserThemeRepository>();
        _handler = new GetUserThemeQueryHandler(_mockUserThemeRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingTheme_ShouldReturnTheme()
    {
        // Arrange
        var userId = 1L;
        var theme = UserTheme.Create(userId, "Dark", "#FF5733", 0.8m, 12, true, true, false);
        
        _mockUserThemeRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(theme);

        var query = new GetUserThemeQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Theme.Should().Be("Dark");
        result.PrimaryColor.Should().Be("#FF5733");
        result.Transparency.Should().Be(0.8m);
        result.BorderRadius.Should().Be(12);
        result.DarkTheme.Should().BeTrue();
        result.PinNavbar.Should().BeTrue();
        result.CompactMode.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNoTheme_ShouldReturnNull()
    {
        // Arrange
        var userId = 1L;
        _mockUserThemeRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTheme?)null);

        var query = new GetUserThemeQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
