using FAM.Application.Auth.ChangePassword;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

using FluentAssertions;

using MediatR;

using Moq;

namespace FAM.Application.Tests.Auth.Handlers;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserDeviceRepository = new Mock<IUserDeviceRepository>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.UserDevices).Returns(_mockUserDeviceRepository.Object);

        _handler = new ChangePasswordCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCurrentPassword_ShouldChangePassword()
    {
        // Arrange
        long userId = 1L;
        string currentPlainPassword = "OldPass123!";
        User user = User.CreateWithPlainPassword("testuser", "test@example.com", currentPlainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        ChangePasswordCommand command = new()
        {
            UserId = userId,
            CurrentPassword = currentPlainPassword, // Use same plain password
            NewPassword = "NewSecurePass123!",
            LogoutAllDevices = false
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        long userId = 999L;
        ChangePasswordCommand command = new()
        {
            UserId = userId,
            CurrentPassword = "OldPass123!",
            NewPassword = "NewSecurePass123!"
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {userId} not found");

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncorrectCurrentPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        long userId = 1L;
        Password currentPassword = Password.Create("CorrectOldPass123!");
        User user = User.Create("testuser", "test@example.com", currentPassword.Hash, currentPassword.Salt, null, null);
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        ChangePasswordCommand command = new()
        {
            UserId = userId,
            CurrentPassword = "WrongOldPass123!", // Wrong password
            NewPassword = "NewSecurePass123!"
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Current password is incorrect");

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithLogoutAllDevices_ShouldDeactivateDevices()
    {
        // Arrange
        long userId = 1L;
        string currentDeviceId = "device123";
        string currentPlainPassword = "OldPass123!";
        User user = User.CreateWithPlainPassword("testuser", "test@example.com", currentPlainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        ChangePasswordCommand command = new()
        {
            UserId = userId,
            CurrentPassword = currentPlainPassword,
            NewPassword = "NewSecurePass123!",
            LogoutAllDevices = true,
            CurrentDeviceId = currentDeviceId
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUserDeviceRepository
            .Setup(x => x.DeactivateAllUserDevicesAsync(userId, currentDeviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _mockUserDeviceRepository.Verify(
            x => x.DeactivateAllUserDevicesAsync(userId, currentDeviceId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithWeakNewPassword_ShouldThrowDomainException()
    {
        // Arrange
        long userId = 1L;
        string currentPlainPassword = "OldPass123!";
        User user = User.CreateWithPlainPassword("testuser", "test@example.com", currentPlainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        ChangePasswordCommand command = new()
        {
            UserId = userId,
            CurrentPassword = currentPlainPassword,
            NewPassword = "weak", // Weak password - no uppercase, no number, no special char, too short
            LogoutAllDevices = false
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();

        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSamePasswordAsOld_ShouldStillSucceed()
    {
        // Arrange
        long userId = 1L;
        string samePlainPassword = "SamePass123!";
        User user = User.CreateWithPlainPassword("testuser", "test@example.com", samePlainPassword);
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        ChangePasswordCommand command = new()
        {
            UserId = userId,
            CurrentPassword = samePlainPassword,
            NewPassword = samePlainPassword, // Same as old - should still work
            LogoutAllDevices = false
        };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()));

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
    }
}
