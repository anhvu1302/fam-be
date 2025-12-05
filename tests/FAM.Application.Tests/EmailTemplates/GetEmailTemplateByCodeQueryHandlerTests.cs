using FAM.Application.EmailTemplates.Queries;
using FAM.Application.EmailTemplates.Queries.GetEmailTemplateByCode;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using Moq;
using Xunit;

namespace FAM.Application.Tests.EmailTemplates;

public class GetEmailTemplateByCodeQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly GetEmailTemplateByCodeQueryHandler _handler;

    public GetEmailTemplateByCodeQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new GetEmailTemplateByCodeQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingCode_ShouldReturnDto()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "OTP_EMAIL", "OTP Email", "Your OTP Code",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        var query = new GetEmailTemplateByCodeQuery("OTP_EMAIL");

        _repositoryMock.Setup(x => x.GetByCodeAsync(query.Code, default))
            .ReturnsAsync(template);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("OTP_EMAIL", result.Code);
        Assert.Equal("OTP Email", result.Name);
        Assert.Equal("Your OTP Code", result.Subject);
        _repositoryMock.Verify(x => x.GetByCodeAsync(query.Code, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCode_ShouldReturnNull()
    {
        // Arrange
        var query = new GetEmailTemplateByCodeQuery("NON_EXISTENT");

        _repositoryMock.Setup(x => x.GetByCodeAsync(query.Code, default))
            .ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithLowercaseCode_ShouldStillWork()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "OTP_EMAIL", "OTP Email", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        var query = new GetEmailTemplateByCodeQuery("otp_email");

        _repositoryMock.Setup(x => x.GetByCodeAsync(query.Code, default))
            .ReturnsAsync(template);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("OTP_EMAIL", result.Code);
        _repositoryMock.Verify(x => x.GetByCodeAsync("otp_email", default), Times.Once);
    }
}
