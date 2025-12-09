using FAM.Application.EmailTemplates.Commands.CreateEmailTemplate;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;
using Moq;

namespace FAM.Application.Tests.EmailTemplates;

public class CreateEmailTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTemplateRepository> _repositoryMock;
    private readonly CreateEmailTemplateCommandHandler _handler;

    public CreateEmailTemplateCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IEmailTemplateRepository>();
        _unitOfWorkMock.Setup(x => x.EmailTemplates).Returns(_repositoryMock.Object);
        _handler = new CreateEmailTemplateCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTemplate()
    {
        // Arrange
        var command = new CreateEmailTemplateCommand
        {
            Code = "TEST_EMAIL",
            Name = "Test Email",
            Subject = "Test Subject",
            HtmlBody = "<html>Test</html>",
            Category = 1,
            IsSystem = false
        };

        _repositoryMock.Setup(x => x.CodeExistsAsync(command.Code, null, default))
            .ReturnsAsync(false);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailTemplate>(), default))
            .Callback<EmailTemplate, CancellationToken>((t, ct) =>
            {
                // Simulate database assigning ID
                var idField = typeof(EmailTemplate).BaseType!.GetProperty("Id");
                idField!.SetValue(t, 1L);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.Equal(1, result);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailTemplate>(), default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateEmailTemplateCommand
        {
            Code = "EXISTING_EMAIL",
            Name = "Test Email",
            Subject = "Test Subject",
            HtmlBody = "<html>Test</html>",
            Category = 1
        };

        _repositoryMock.Setup(x => x.CodeExistsAsync(command.Code, null, default))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, default));

        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_CODE_EXISTS, exception.ErrorCode);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailTemplate>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAllOptionalParameters_ShouldCreateTemplate()
    {
        // Arrange
        var command = new CreateEmailTemplateCommand
        {
            Code = "TEST_EMAIL",
            Name = "Test Email",
            Subject = "Test Subject",
            HtmlBody = "<html>Test</html>",
            PlainTextBody = "Plain text",
            Description = "Test description",
            AvailablePlaceholders = "[\"userName\",\"email\"]",
            Category = 2,
            IsSystem = true
        };

        _repositoryMock.Setup(x => x.CodeExistsAsync(command.Code, null, default))
            .ReturnsAsync(false);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailTemplate>(), default))
            .Callback<EmailTemplate, CancellationToken>((t, ct) =>
            {
                var idField = typeof(EmailTemplate).BaseType!.GetProperty("Id");
                idField!.SetValue(t, 1L);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.Equal(1, result);
        _repositoryMock.Verify(x => x.AddAsync(It.Is<EmailTemplate>(t =>
            t.Code == "TEST_EMAIL" &&
            t.PlainTextBody == "Plain text" &&
            t.Description == "Test description" &&
            t.IsSystem == true
        ), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldConvertCodeToUppercase()
    {
        // Arrange
        var command = new CreateEmailTemplateCommand
        {
            Code = "test_email",
            Name = "Test Email",
            Subject = "Test Subject",
            HtmlBody = "<html>Test</html>",
            Category = 1
        };

        _repositoryMock.Setup(x => x.CodeExistsAsync("test_email", null, default))
            .ReturnsAsync(false);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailTemplate>(), default))
            .Callback<EmailTemplate, CancellationToken>((t, ct) =>
            {
                var idField = typeof(EmailTemplate).BaseType!.GetProperty("Id");
                idField!.SetValue(t, 1L);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.Is<EmailTemplate>(t =>
            t.Code == "TEST_EMAIL"
        ), default), Times.Once);
    }
}