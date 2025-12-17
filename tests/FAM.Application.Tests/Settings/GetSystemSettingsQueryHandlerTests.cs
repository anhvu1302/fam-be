using System.Linq.Expressions;

using FAM.Application.Querying;
using FAM.Application.Querying.Parsing;
using FAM.Application.Settings.Queries.GetSystemSettings;
using FAM.Application.Settings.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using Moq;

namespace FAM.Application.Tests.Settings;

public class GetSystemSettingsQueryHandlerTests
{
    private readonly Mock<ISystemSettingRepository> _repositoryMock;
    private readonly Mock<IFilterParser> _filterParserMock;
    private readonly GetSystemSettingsQueryHandler _handler;

    public GetSystemSettingsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ISystemSettingRepository>();
        _filterParserMock = new Mock<IFilterParser>();
        _handler = new GetSystemSettingsQueryHandler(_repositoryMock.Object, _filterParserMock.Object);
    }

    [Fact]
    public async Task Handle_WithoutFilter_ShouldReturnPagedResults()
    {
        // Arrange
        var settings = new List<SystemSetting>
        {
            SystemSetting.Create("setting1", "Setting 1"),
            SystemSetting.Create("setting2", "Setting 2")
        };

        var queryRequest = new QueryRequest { Page = 1, PageSize = 10 };
        var query = new GetSystemSettingsQuery(queryRequest);

        _repositoryMock.Setup(x => x.GetPagedAsync(
                null,
                It.IsAny<string>(),
                1,
                10,
                It.IsAny<Expression<Func<SystemSetting, object>>[]>(),
                default))
            .ReturnsAsync((settings, 2));

        // Act
        PageResult<SystemSettingDto> result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("setting1", result.Items[0].Key);
        Assert.Equal("setting2", result.Items[1].Key);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var settings = new List<SystemSetting>
        {
            SystemSetting.Create("setting3", "Setting 3"),
            SystemSetting.Create("setting4", "Setting 4")
        };

        var queryRequest = new QueryRequest { Page = 2, PageSize = 2 };
        var query = new GetSystemSettingsQuery(queryRequest);

        _repositoryMock.Setup(x => x.GetPagedAsync(
                null,
                It.IsAny<string>(),
                2,
                2,
                It.IsAny<Expression<Func<SystemSetting, object>>[]>(),
                default))
            .ReturnsAsync((settings, 4));

        // Act
        PageResult<SystemSettingDto> result = await _handler.Handle(query, default);

        // Assert
        Assert.Equal(4, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPage()
    {
        // Arrange
        var queryRequest = new QueryRequest { Page = 1, PageSize = 10 };
        var query = new GetSystemSettingsQuery(queryRequest);

        _repositoryMock.Setup(x => x.GetPagedAsync(
                null,
                It.IsAny<string>(),
                1,
                10,
                It.IsAny<Expression<Func<SystemSetting, object>>[]>(),
                default))
            .ReturnsAsync((new List<SystemSetting>(), 0));

        // Act
        PageResult<SystemSettingDto> result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Items);
    }
}
