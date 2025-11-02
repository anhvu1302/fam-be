using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Application.Users.Commands;
using FAM.Application.Users.Handlers;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace FAM.Application.Tests.Users.Handlers;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        _handler = new UpdateUserCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateUser()
    {
        // Arrange
        var userId = 1L;
        var existingUser = User.Create("olduser", "old@example.com", "OldPass123!", null, null, null);
        typeof(User).GetProperty("Id")?.SetValue(existingUser, userId);

        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "newuser",
            Email = "new@example.com",
            Password = "NewPass123!",
            FullName = "New User"
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.IsEmailTakenAsync(command.Email, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var expectedDto = new UserDto
        {
            Id = userId,
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
        result.Id.Should().Be(userId);
        result.Username.Should().Be(command.Username);
        result.Email.Should().Be(command.Email);

        _mockUserRepository.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(command.Username, userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsEmailTakenAsync(command.Email, userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()), 
            Times.Once);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = 999L;
        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {userId} not found");

        _mockUserRepository.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        _mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithTakenUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1L;
        var existingUser = User.Create("olduser", "old@example.com", "OldPass123!", null, null, null);
        typeof(User).GetProperty("Id")?.SetValue(existingUser, userId);

        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "takenusername",
            Email = "new@example.com",
            Password = "NewPass123!",
            FullName = "Test User"
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Username is already taken");

        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(command.Username, userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        _mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithTakenEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1L;
        var existingUser = User.Create("olduser", "old@example.com", "OldPass123!", null, null, null);
        typeof(User).GetProperty("Id")?.SetValue(existingUser, userId);

        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "newuser",
            Email = "taken@example.com",
            Password = "NewPass123!",
            FullName = "Test User"
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.IsUsernameTakenAsync(command.Username, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.IsEmailTakenAsync(command.Email, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email is already taken");

        _mockUserRepository.Verify(
            x => x.IsUsernameTakenAsync(command.Username, userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.IsEmailTakenAsync(command.Email, userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()), 
            Times.Never);
    }
}
