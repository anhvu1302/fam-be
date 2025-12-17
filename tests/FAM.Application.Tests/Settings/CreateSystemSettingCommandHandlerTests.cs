using System.Reflection;

using FAM.Application.Settings.Commands.CreateSystemSetting;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;

using Moq;

namespace FAM.Application.Tests.Settings;

public class CreateSystemSettingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemSettingRepository> _repositoryMock;
    private readonly CreateSystemSettingCommandHandler _handler;

    public CreateSystemSettingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _unitOfWorkMock.Setup(x => x.SystemSettings).Returns(_repositoryMock.Object);
        _handler = new CreateSystemSettingCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSetting()
    {
        // Arrange
        var command = new CreateSystemSettingCommand
        {
            Key = "test_setting",
            DisplayName = "Test Setting",
            Value = "test value",
            DefaultValue = "default",
            DataType = SettingDataType.String,
            Group = "general",
            Description = "Test description",
            SortOrder = 1,
            IsRequired = false,
            IsSensitive = false,
            IsVisible = true,
            IsEditable = true
        };

        _repositoryMock.Setup(x => x.GetByKeyAsync(command.Key, default))
            .ReturnsAsync((SystemSetting?)null);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<SystemSetting>(), default))
            .Callback<SystemSetting, CancellationToken>((s, ct) =>
            {
                PropertyInfo? idField = typeof(SystemSetting).BaseType!.GetProperty("Id");
                idField!.SetValue(s, 1L);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.Equal(1, result);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<SystemSetting>(), default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateKey_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateSystemSettingCommand
        {
            Key = "existing_key",
            DisplayName = "Test Setting",
            DataType = SettingDataType.String
        };

        var existingSetting = SystemSetting.Create("existing_key", "Existing Setting");
        _repositoryMock.Setup(x => x.GetByKeyAsync(command.Key, default))
            .ReturnsAsync(existingSetting);

        // Act & Assert
        ConflictException exception =
            await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.SETTING_KEY_EXISTS, exception.ErrorCode);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<SystemSetting>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidationRulesAndOptions_ShouldCreateSetting()
    {
        // Arrange
        var command = new CreateSystemSettingCommand
        {
            Key = "test_setting",
            DisplayName = "Test Setting",
            DataType = SettingDataType.Select,
            ValidationRules = "{\"required\":true}",
            Options = "[\"option1\",\"option2\"]"
        };

        _repositoryMock.Setup(x => x.GetByKeyAsync(command.Key, default))
            .ReturnsAsync((SystemSetting?)null);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<SystemSetting>(), default))
            .Callback<SystemSetting, CancellationToken>((s, ct) =>
            {
                PropertyInfo? idField = typeof(SystemSetting).BaseType!.GetProperty("Id");
                idField!.SetValue(s, 1L);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.Equal(1, result);
        _repositoryMock.Verify(x => x.AddAsync(It.Is<SystemSetting>(s =>
            s.ValidationRules == command.ValidationRules &&
            s.Options == command.Options
        ), default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSensitiveSetting_ShouldCreateWithCorrectFlags()
    {
        // Arrange
        var command = new CreateSystemSettingCommand
        {
            Key = "api_key",
            DisplayName = "API Key",
            IsSensitive = true,
            IsRequired = true,
            IsVisible = false,
            IsEditable = false
        };

        _repositoryMock.Setup(x => x.GetByKeyAsync(command.Key, default))
            .ReturnsAsync((SystemSetting?)null);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<SystemSetting>(), default))
            .Callback<SystemSetting, CancellationToken>((s, ct) =>
            {
                PropertyInfo? idField = typeof(SystemSetting).BaseType!.GetProperty("Id");
                idField!.SetValue(s, 1L);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.Equal(1, result);
        _repositoryMock.Verify(x => x.AddAsync(It.Is<SystemSetting>(s =>
            s.IsSensitive &&
            s.IsRequired &&
            !s.IsVisible &&
            !s.IsEditable
        ), default), Times.Once);
    }
}
