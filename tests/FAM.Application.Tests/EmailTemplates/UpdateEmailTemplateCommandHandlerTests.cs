using FAM.Application.EmailTemplates.Commands.UpdateEmailTemplate;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using Moq;

namespace FAM.Application.Tests.EmailTemplates;

public class UpdateEmailTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly UpdateEmailTemplateCommandHandler _handler;

    public UpdateEmailTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new UpdateEmailTemplateCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateTemplate()
    {
        // Arrange
        EmailTemplate existingTemplate = EmailTemplate.Create(
            "TEST_EMAIL", "Original Name", "Original Subject",
            "<html>Original</html>", EmailTemplateCategory.Authentication);

        UpdateEmailTemplateCommand command = new()
        {
            Id = 1,
            Name = "Updated Name",
            Subject = "Updated Subject",
            HtmlBody = "<html>Updated</html>",
            Category = 2
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(existingTemplate);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.Equal(command.Name, existingTemplate.Name);
        Assert.Equal(command.Subject, existingTemplate.Subject);
        Assert.Equal(command.HtmlBody, existingTemplate.HtmlBody);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        UpdateEmailTemplateCommand command = new()
        {
            Id = 999,
            Name = "Updated Name",
            Subject = "Updated Subject",
            HtmlBody = "<html>Updated</html>",
            Category = 1
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync((EmailTemplate?)null);

        // Act & Assert
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND, exception.ErrorCode);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSystemTemplate_ShouldThrowDomainException()
    {
        // Arrange
        EmailTemplate existingTemplate = EmailTemplate.Create(
            "SYSTEM_EMAIL", "System Template", "Subject",
            "<html>Body</html>", EmailTemplateCategory.System,
            isSystem: true);

        UpdateEmailTemplateCommand command = new()
        {
            Id = 1,
            Name = "Updated Name",
            Subject = "Updated Subject",
            HtmlBody = "<html>Updated</html>",
            Category = 1
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(existingTemplate);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE, exception.ErrorCode);
    }

    [Fact]
    public async Task Handle_WithAllOptionalParameters_ShouldUpdateAll()
    {
        // Arrange
        EmailTemplate existingTemplate = EmailTemplate.Create(
            "TEST_EMAIL", "Original", "Subject",
            "<html>Original</html>", EmailTemplateCategory.Authentication);

        UpdateEmailTemplateCommand command = new()
        {
            Id = 1,
            Name = "Updated Name",
            Subject = "Updated Subject",
            HtmlBody = "<html>Updated</html>",
            PlainTextBody = "Plain text",
            Description = "New description",
            AvailablePlaceholders = "[\"test\"]",
            Category = 2
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(existingTemplate);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.Equal(command.PlainTextBody, existingTemplate.PlainTextBody);
        Assert.Equal(command.Description, existingTemplate.Description);
        Assert.Equal(command.AvailablePlaceholders, existingTemplate.AvailablePlaceholders);
    }
}
