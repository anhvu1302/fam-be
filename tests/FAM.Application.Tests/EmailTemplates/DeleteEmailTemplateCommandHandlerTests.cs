using FAM.Application.EmailTemplates.Commands.DeleteEmailTemplate;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using Moq;

namespace FAM.Application.Tests.EmailTemplates;

public class DeleteEmailTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly DeleteEmailTemplateCommandHandler _handler;

    public DeleteEmailTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new DeleteEmailTemplateCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidTemplate_ShouldSoftDelete()
    {
        // Arrange
        EmailTemplate template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        DeleteEmailTemplateCommand command = new(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(template);

        // Act
        await _handler.Handle(command, default);

        // Assert
        Assert.True(template.IsDeleted);
        Assert.NotNull(template.DeletedAt);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        DeleteEmailTemplateCommand command = new(999);

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
        EmailTemplate template = EmailTemplate.Create(
            "SYSTEM_EMAIL", "System Template", "Subject",
            "<html>Body</html>", EmailTemplateCategory.System,
            isSystem: true);

        DeleteEmailTemplateCommand command = new(1);

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, default))
            .ReturnsAsync(template);

        // Act & Assert
        DomainException exception = await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE, exception.ErrorCode);
    }
}
