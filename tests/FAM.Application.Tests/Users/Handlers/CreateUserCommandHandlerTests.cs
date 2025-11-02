using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Application.Users.Commands;
using FAM.Application.Users.Handlers;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FluentAssertions;
using Moq;
using Xunit;

namespace FAM.Application.Tests.Users.Handlers;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new CreateUserCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

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

        var expectedDto = new UserDto
        {
            Id = 1,
            Username = command.Username,
            Email = command.Email,
            FullName = command.FullName
        };

        _mockMapper
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(command.Username);
        result.Email.Should().Be(command.Email);
        result.FullName.Should().Be(command.FullName);

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
    public async Task Handle_WithTakenUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Username is already taken");

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
    public async Task Handle_WithTakenEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.IsEmailTakenAsync(command.Email, It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email is already taken");

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
