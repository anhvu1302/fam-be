using FAM.Application.EmailTemplates.Queries;
using FAM.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using Moq;
using Xunit;

namespace FAM.Application.Tests.EmailTemplates;

public class GetAllEmailTemplatesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly GetAllEmailTemplatesQueryHandler _handler;

    public GetAllEmailTemplatesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new GetAllEmailTemplatesQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithoutFilters_ShouldReturnAllTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplate>
        {
            EmailTemplate.Create("EMAIL1", "Name 1", "Subject 1", "<html>1</html>", EmailTemplateCategory.Authentication),
            EmailTemplate.Create("EMAIL2", "Name 2", "Subject 2", "<html>2</html>", EmailTemplateCategory.Notification)
        };

        var query = new GetAllEmailTemplatesQuery();

        _repositoryMock.Setup(x => x.GetAllAsync(default))
            .ReturnsAsync(templates);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("EMAIL1", result[0].Code);
        Assert.Equal("EMAIL2", result[1].Code);
        _repositoryMock.Verify(x => x.GetAllAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_ShouldReturnActiveTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplate>
        {
            EmailTemplate.Create("EMAIL1", "Active", "Subject", "<html>1</html>", EmailTemplateCategory.Authentication)
        };

        var query = new GetAllEmailTemplatesQuery { IsActive = true };

        _repositoryMock.Setup(x => x.GetActiveTemplatesAsync(default))
            .ReturnsAsync(templates);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EMAIL1", result[0].Code);
        Assert.True(result[0].IsActive);
        _repositoryMock.Verify(x => x.GetActiveTemplatesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplate>
        {
            EmailTemplate.Create("EMAIL1", "Auth", "Subject", "<html>1</html>", EmailTemplateCategory.Authentication)
        };

        var query = new GetAllEmailTemplatesQuery { Category = 1 }; // Authentication

        _repositoryMock.Setup(x => x.GetByCategoryAsync(EmailTemplateCategory.Authentication, default))
            .ReturnsAsync(templates);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EMAIL1", result[0].Code);
        Assert.Equal(1, result[0].Category);
        _repositoryMock.Verify(x => x.GetByCategoryAsync(EmailTemplateCategory.Authentication, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithBothFilters_ShouldReturnFilteredTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplate>
        {
            EmailTemplate.Create("EMAIL1", "Auth Active", "Subject", "<html>1</html>", EmailTemplateCategory.Authentication)
        };

        var query = new GetAllEmailTemplatesQuery 
        { 
            IsActive = true, 
            Category = 1 
        };

        _repositoryMock.Setup(x => x.GetByCategoryAsync(EmailTemplateCategory.Authentication, default))
            .ReturnsAsync(templates);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EMAIL1", result[0].Code);
        Assert.True(result[0].IsActive);
        _repositoryMock.Verify(x => x.GetByCategoryAsync(EmailTemplateCategory.Authentication, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoMatchingTemplates_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllEmailTemplatesQuery { Category = 99 };

        _repositoryMock.Setup(x => x.GetByCategoryAsync(It.IsAny<EmailTemplateCategory>(), default))
            .ReturnsAsync(new List<EmailTemplate>());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
