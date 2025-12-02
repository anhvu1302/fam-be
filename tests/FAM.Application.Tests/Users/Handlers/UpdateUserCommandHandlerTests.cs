using FAM.Application.Users.Commands.UpdateUser;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FluentAssertions;
using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new UpdateUserCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var userId = 1L;
        var existingUser = User.Create(
            "olduser",
            "old@example.com",
            "OldPass123!",
            firstName: "Old",
            lastName: "User"
        );

        var command = new UpdateUserCommand(
            userId,
            "newuser",
            "new@example.com",
            FirstName: "New",
            LastName: "User"
        );

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username!, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.IsEmailTakenAsync(command.Email!, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockUserRepository.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = 999L;
        var command = new UpdateUserCommand(
            userId,
            "newuser"
        );

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _mockUserRepository.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);
    }
}