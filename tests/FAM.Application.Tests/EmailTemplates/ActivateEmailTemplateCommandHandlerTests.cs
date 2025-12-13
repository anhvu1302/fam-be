using FAM.Application.EmailTemplates.Commands.ActivateEmailTemplate;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using Moq;

namespace FAM.Application.Tests.EmailTemplates;

public class ActivateEmailTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly ActivateEmailTemplateCommandHandler _handler;

    public ActivateEmailTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new ActivateEmailTemplateCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidTemplate_ShouldActivate()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);
        template.Deactivate(); // Deactivate first

        var command = new ActivateEmailTemplateCommand(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(template);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.True(template.IsActive);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new ActivateEmailTemplateCommand(999);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync((EmailTemplate?)null);

        // Act & Assert
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND, exception.ErrorCode);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyActiveTemplate_ShouldStillSucceed()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);
        // Template is active by default

        var command = new ActivateEmailTemplateCommand(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(template);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.True(template.IsActive);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
