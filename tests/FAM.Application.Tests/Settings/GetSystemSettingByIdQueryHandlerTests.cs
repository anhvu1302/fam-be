using FAM.Application.Settings.Queries.GetSystemSettingById;
using FAM.Application.Settings.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using Moq;

namespace FAM.Application.Tests.Settings;

public class GetSystemSettingByIdQueryHandlerTests
{
    private readonly Mock<ISystemSettingRepository> _repositoryMock;
    private readonly GetSystemSettingByIdQueryHandler _handler;

    public GetSystemSettingByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _handler = new GetSystemSettingByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingSetting_ShouldReturnDto()
    {
        // Arrange
        var setting = SystemSetting.Create(
            "test_key",
            "Test Setting",
            "test value",
            "default value",
            SettingDataType.String,
            "general",
            "Test description",
            1,
            false,
            false);

        var query = new GetSystemSettingByIdQuery(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(query.Id, default))
            .ReturnsAsync(setting);

        // Act
        SystemSettingDto? result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test_key", result.Key);
        Assert.Equal("Test Setting", result.DisplayName);
        Assert.Equal("test value", result.Value);
        Assert.Equal("default value", result.DefaultValue);
        Assert.Equal("test value", result.EffectiveValue);
        Assert.Equal(SettingDataType.String, result.DataType);
        Assert.Equal("general", result.Group);
        _repositoryMock.Verify(x => x.GetByIdAsync(query.Id, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentSetting_ShouldReturnNull()
    {
        // Arrange
        var query = new GetSystemSettingByIdQuery(999);

        _repositoryMock.Setup(x => x.GetByIdAsync(query.Id, default))
            .ReturnsAsync((SystemSetting?)null);

        // Act
        SystemSettingDto? result = await _handler.Handle(query, default);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithSettingWithoutValue_ShouldReturnDefaultAsEffective()
    {
        // Arrange
        var setting = SystemSetting.Create(
            "test_key",
            "Test Setting",
            null,
            "default value");

        var query = new GetSystemSettingByIdQuery(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(query.Id, default))
            .ReturnsAsync(setting);

        // Act
        SystemSettingDto? result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.Equal("default value", result.DefaultValue);
        Assert.Equal("default value", result.EffectiveValue);
    }
}
