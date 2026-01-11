using FAM.Application.Settings.Queries.GetPublicSettings;
using FAM.Application.Settings.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using Moq;

namespace FAM.Application.Tests.Settings;

public class GetPublicSettingsQueryHandlerTests
{
    private readonly Mock<ISystemSettingRepository> _repositoryMock;
    private readonly GetPublicSettingsQueryHandler _handler;

    public GetPublicSettingsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _handler = new GetPublicSettingsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyNonSensitiveSettings()
    {
        // Arrange
        List<SystemSetting> settings = new()
        {
            SystemSetting.Create("public_setting", "Public Setting", "value1", null, SettingDataType.String, "general",
                null, 0, false, false),
            SystemSetting.Create("sensitive_setting", "Sensitive Setting", "secret", null, SettingDataType.String,
                "security", null, 0, false, true),
            SystemSetting.Create("another_public", "Another Public", "value2", null, SettingDataType.String, "general",
                null, 0, false, false)
        };

        GetPublicSettingsQuery query = new();

        _repositoryMock.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(settings);

        // Act
        IEnumerable<PublicSettingDto> result = await _handler.Handle(query, default);

        // Assert
        List<PublicSettingDto> resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, setting => Assert.False(settings.First(s => s.Key == setting.Key).IsSensitive));
        Assert.Contains(resultList, s => s.Key == "public_setting");
        Assert.Contains(resultList, s => s.Key == "another_public");
        Assert.DoesNotContain(resultList, s => s.Key == "sensitive_setting");
    }

    [Fact]
    public async Task Handle_WithNoSettings_ShouldReturnEmptyList()
    {
        // Arrange
        GetPublicSettingsQuery query = new();

        _repositoryMock.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(new List<SystemSetting>());

        // Act
        IEnumerable<PublicSettingDto> result = await _handler.Handle(query, default);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithAllSensitiveSettings_ShouldReturnEmptyList()
    {
        // Arrange
        List<SystemSetting> settings = new()
        {
            SystemSetting.Create("secret1", "Secret 1", "value", null, SettingDataType.String, "security", null, 0,
                false, true),
            SystemSetting.Create("secret2", "Secret 2", "value", null, SettingDataType.String, "security", null, 0,
                false, true)
        };

        GetPublicSettingsQuery query = new();

        _repositoryMock.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(settings);

        // Act
        IEnumerable<PublicSettingDto> result = await _handler.Handle(query, default);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEffectiveValues()
    {
        // Arrange
        List<SystemSetting> settings = new()
        {
            SystemSetting.Create("setting_with_value", "Setting With Value", "custom", "default"),
            SystemSetting.Create("setting_without_value", "Setting Without Value", null, "default_value")
        };

        GetPublicSettingsQuery query = new();

        _repositoryMock.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(settings);

        // Act
        IEnumerable<PublicSettingDto> result = await _handler.Handle(query, default);

        // Assert
        List<PublicSettingDto> resultList = result.ToList();
        Assert.Equal(2, resultList.Count);

        PublicSettingDto settingWithValue = resultList.First(s => s.Key == "setting_with_value");
        Assert.Equal("custom", settingWithValue.Value);

        PublicSettingDto settingWithoutValue = resultList.First(s => s.Key == "setting_without_value");
        Assert.Equal("default_value", settingWithoutValue.Value);
    }
}
