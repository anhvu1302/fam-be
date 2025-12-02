using FAM.Application.Users.Commands.CreateUser;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FluentAssertions;
using Moq;

namespace FAM.Application.Tests.Users.Handlers;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new CreateUserCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            "testuser",
            "test@example.com",
            "SecurePass123!",
            "Test",
            "User"
        );

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.IsEmailTakenAsync(command.Email, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.User.Username.Should().Be(command.Username);
        result.Value!.User.Email.Should().Be(command.Email);

        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsEmailTakenAsync(command.Email, It.IsAny<long?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithTakenUsername_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            "existinguser",
            "test@example.com",
            "SecurePass123!"
        );

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Username is already taken");

        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUserRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithTakenEmail_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand(
            "testuser",
            "existing@example.com",
            "SecurePass123!"
        );

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.IsEmailTakenAsync(command.Email, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already taken");

        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsEmailTakenAsync(command.Email, It.IsAny<long?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}