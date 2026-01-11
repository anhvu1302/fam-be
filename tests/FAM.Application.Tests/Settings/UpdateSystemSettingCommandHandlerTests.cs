using FAM.Application.Settings.Commands.UpdateSystemSetting;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;

using Moq;

namespace FAM.Application.Tests.Settings;

public class UpdateSystemSettingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemSettingRepository> _repositoryMock;
    private readonly UpdateSystemSettingCommandHandler _handler;

    public UpdateSystemSettingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _unitOfWorkMock.Setup(x => x.SystemSettings).Returns(_repositoryMock.Object);
        _handler = new UpdateSystemSettingCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSetting()
    {
        // Arrange
        SystemSetting setting = SystemSetting.Create("test_key", "Original Name");
        UpdateSystemSettingCommand command = new()
        {
            Id = 1,
            DisplayName = "Updated Name",
            Value = "new value",
            Description = "Updated description",
            SortOrder = 10
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(setting);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.Equal("Updated Name", setting.DisplayName);
        Assert.Equal("Updated description", setting.Description);
        Assert.Equal(10, setting.SortOrder);
        _repositoryMock.Verify(x => x.Update(setting), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentSetting_ShouldThrowNotFoundException()
    {
        // Arrange
        UpdateSystemSettingCommand command = new()
        {
            Id = 999,
            DisplayName = "Updated Name"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync((SystemSetting?)null);

        // Act & Assert
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.SETTING_NOT_FOUND, exception.ErrorCode);
        _repositoryMock.Verify(x => x.Update(It.IsAny<SystemSetting>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValueUpdate_ShouldSetNewValue()
    {
        // Arrange
        SystemSetting setting = SystemSetting.Create("test_key", "Test Setting");
        UpdateSystemSettingCommand command = new()
        {
            Id = 1,
            DisplayName = "Test Setting",
            Value = "new value"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(setting);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.Equal("new value", setting.Value);
        _repositoryMock.Verify(x => x.Update(setting), Times.Once);
    }

    [Fact]
    public async Task Handle_WithVisibilityChange_ShouldUpdateVisibility()
    {
        // Arrange
        SystemSetting setting = SystemSetting.Create("test_key", "Test Setting");
        UpdateSystemSettingCommand command = new()
        {
            Id = 1,
            DisplayName = "Test Setting",
            IsVisible = false
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(setting);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.False(setting.IsVisible);
        _repositoryMock.Verify(x => x.Update(setting), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEditabilityChange_ShouldUpdateEditability()
    {
        // Arrange
        SystemSetting setting = SystemSetting.Create("test_key", "Test Setting");
        UpdateSystemSettingCommand command = new()
        {
            Id = 1,
            DisplayName = "Test Setting",
            IsEditable = false
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(setting);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.False(setting.IsEditable);
        _repositoryMock.Verify(x => x.Update(setting), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidationRulesAndOptions_ShouldUpdateBoth()
    {
        // Arrange
        SystemSetting setting = SystemSetting.Create("test_key", "Test Setting");
        UpdateSystemSettingCommand command = new()
        {
            Id = 1,
            DisplayName = "Test Setting",
            ValidationRules = "{\"required\":true}",
            Options = "[\"option1\",\"option2\"]"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(setting);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.Equal("{\"required\":true}", setting.ValidationRules);
        Assert.Equal("[\"option1\",\"option2\"]", setting.Options);
        _repositoryMock.Verify(x => x.Update(setting), Times.Once);
    }
}
