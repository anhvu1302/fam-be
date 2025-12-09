using FAM.Application.EmailTemplates.Queries.GetEmailTemplateById;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using Moq;

namespace FAM.Application.Tests.EmailTemplates;

public class GetEmailTemplateByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly GetEmailTemplateByIdQueryHandler _handler;

    public GetEmailTemplateByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new GetEmailTemplateByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingTemplate_ShouldReturnDto()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test Email", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        var query = new GetEmailTemplateByIdQuery(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(query.Id, default))
            .ReturnsAsync(template);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST_EMAIL", result.Code);
        Assert.Equal("Test Email", result.Name);
        Assert.Equal("Subject", result.Subject);
        _repositoryMock.Verify(x => x.GetByIdAsync(query.Id, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldReturnNull()
    {
        // Arrange
        var query = new GetEmailTemplateByIdQuery(999);

        _repositoryMock.Setup(x => x.GetByIdAsync(query.Id, default))
            .ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.Null(result);
    }
}