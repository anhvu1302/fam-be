using FAM.Application.Users.Commands.UpdateUserTheme;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users.Entities;

using FluentAssertions;

using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class UpdateUserThemeCommandHandlerTests
{
    private readonly Mock<IUserThemeRepository> _mockUserThemeRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateUserThemeCommandHandler _handler;

    public UpdateUserThemeCommandHandlerTests()
    {
        _mockUserThemeRepository = new Mock<IUserThemeRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new UpdateUserThemeCommandHandler(
            _mockUserThemeRepository.Object,
            _mockUserRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithNewTheme_ShouldCreateTheme()
    {
        // Arrange
        long userId = 1L;
        _mockUserRepository
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserThemeRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTheme?)null);

        UpdateUserThemeCommand command = new(
            userId, "Dark", "#FF5733", 0.8m, 12, true, true, false);

        // Act
        UpdateUserThemeResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Theme.Should().Be("Dark");
        result.PrimaryColor.Should().Be("#FF5733");
        result.Transparency.Should().Be(0.8m);
        result.BorderRadius.Should().Be(12);
        result.DarkTheme.Should().BeTrue();

        _mockUserThemeRepository.Verify(
            x => x.AddAsync(It.IsAny<UserTheme>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingTheme_ShouldUpdateTheme()
    {
        // Arrange
        long userId = 1L;
        UserTheme existingTheme = UserTheme.CreateDefault(userId);

        _mockUserRepository
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserThemeRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTheme);

        UpdateUserThemeCommand command = new(
            userId, "BlueJelly", "#3B82F6", 0.7m, 16, true, false, true);

        // Act
        UpdateUserThemeResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Theme.Should().Be("BlueJelly");
        result.PrimaryColor.Should().Be("#3B82F6");
        result.Transparency.Should().Be(0.7m);
        result.BorderRadius.Should().Be(16);
        result.DarkTheme.Should().BeTrue();
        result.PinNavbar.Should().BeFalse();
        result.CompactMode.Should().BeTrue();

        _mockUserThemeRepository.Verify(x => x.Update(It.IsAny<UserTheme>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowDomainException()
    {
        // Arrange
        long userId = 999L;
        _mockUserRepository
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UpdateUserThemeCommand command = new(
            userId, "Dark", null, 0.5m, 8, false, false, false);

        // Act & Assert
        DomainException exception =
            await Assert.ThrowsAsync<DomainException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

        exception.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }
}
