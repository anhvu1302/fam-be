using FAM.Domain.Common;
using FAM.Domain.EmailTemplates;
using Xunit;

namespace FAM.Domain.Tests.EmailTemplates;

public class EmailTemplateTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        const string code = "TEST_EMAIL";
        const string name = "Test Email Template";
        const string subject = "Test Subject";
        const string htmlBody = "<html><body>Test</body></html>";
        const EmailTemplateCategory category = EmailTemplateCategory.Authentication;

        // Act
        var template = EmailTemplate.Create(code, name, subject, htmlBody, category);

        // Assert
        Assert.NotNull(template);
        Assert.Equal(code, template.Code);
        Assert.Equal(name, template.Name);
        Assert.Equal(subject, template.Subject);
        Assert.Equal(htmlBody, template.HtmlBody);
        Assert.Equal(category, template.Category);
        Assert.True(template.IsActive);
        Assert.False(template.IsSystem);
        Assert.False(template.IsDeleted);
    }

    [Fact]
    public void Create_WithAllOptionalParameters_ShouldSucceed()
    {
        // Arrange
        const string code = "TEST_EMAIL";
        const string name = "Test Email Template";
        const string subject = "Test Subject";
        const string htmlBody = "<html><body>Test</body></html>";
        const string description = "Test description";
        const string plainTextBody = "Plain text version";
        const string placeholders = "[\"userName\",\"email\"]";
        const EmailTemplateCategory category = EmailTemplateCategory.Notification;

        // Act
        var template = EmailTemplate.Create(
            code, name, subject, htmlBody, category,
            description, plainTextBody, placeholders, isSystem: true);

        // Assert
        Assert.Equal(description, template.Description);
        Assert.Equal(plainTextBody, template.PlainTextBody);
        Assert.Equal(placeholders, template.AvailablePlaceholders);
        Assert.True(template.IsSystem);
    }

    [Fact]
    public void Create_WithEmptyCode_ShouldThrowException()
    {
        // Arrange
        const string code = "";
        const string name = "Test";
        const string subject = "Subject";
        const string htmlBody = "<html></html>";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            EmailTemplate.Create(code, name, subject, htmlBody, EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_CODE_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Create_WithWhitespaceCode_ShouldThrowException()
    {
        // Arrange
        const string code = "   ";
        const string name = "Test";
        const string subject = "Subject";
        const string htmlBody = "<html></html>";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            EmailTemplate.Create(code, name, subject, htmlBody, EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_CODE_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        const string code = "TEST_EMAIL";
        const string name = "";
        const string subject = "Subject";
        const string htmlBody = "<html></html>";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            EmailTemplate.Create(code, name, subject, htmlBody, EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptySubject_ShouldThrowException()
    {
        // Arrange
        const string code = "TEST_EMAIL";
        const string name = "Test";
        const string subject = "";
        const string htmlBody = "<html></html>";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            EmailTemplate.Create(code, name, subject, htmlBody, EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Create_WithEmptyHtmlBody_ShouldThrowException()
    {
        // Arrange
        const string code = "TEST_EMAIL";
        const string name = "Test";
        const string subject = "Subject";
        const string htmlBody = "";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            EmailTemplate.Create(code, name, subject, htmlBody, EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Create_ShouldConvertCodeToUppercase()
    {
        // Arrange
        const string code = "test_email";
        const string name = "Test";
        const string subject = "Subject";
        const string htmlBody = "<html></html>";

        // Act
        var template = EmailTemplate.Create(code, name, subject, htmlBody, EmailTemplateCategory.Authentication);

        // Assert
        Assert.Equal("TEST_EMAIL", template.Code);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldSucceed()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Original", "Original Subject",
            "<html>Original</html>", EmailTemplateCategory.Authentication);

        const string newName = "Updated Name";
        const string newSubject = "Updated Subject";
        const string newHtmlBody = "<html>Updated</html>";
        const EmailTemplateCategory newCategory = EmailTemplateCategory.Notification;

        // Act
        template.Update(newName, newSubject, newHtmlBody, newCategory);

        // Assert
        Assert.Equal(newName, template.Name);
        Assert.Equal(newSubject, template.Subject);
        Assert.Equal(newHtmlBody, template.HtmlBody);
        Assert.Equal(newCategory, template.Category);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Update_WithOptionalParameters_ShouldSucceed()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Original", "Subject",
            "<html>Original</html>", EmailTemplateCategory.Authentication);

        const string description = "New description";
        const string plainText = "Plain text";
        const string placeholders = "[\"test\"]";

        // Act
        template.Update("Updated", "Subject", "<html>Updated</html>",
            EmailTemplateCategory.Notification, description, plainText, placeholders);

        // Assert
        Assert.Equal(description, template.Description);
        Assert.Equal(plainText, template.PlainTextBody);
        Assert.Equal(placeholders, template.AvailablePlaceholders);
    }

    [Fact]
    public void Update_SystemTemplate_ShouldThrowException()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "SYSTEM_EMAIL", "System Template", "Subject",
            "<html>Body</html>", EmailTemplateCategory.System,
            isSystem: true);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            template.Update("New Name", "Subject", "<html>Body</html>", EmailTemplateCategory.System));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE, exception.ErrorCode);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Original", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            template.Update("", "Subject", "<html>Body</html>", EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Update_WithEmptySubject_ShouldThrowException()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Name", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            template.Update("Name", "", "<html>Body</html>", EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED, exception.ErrorCode);
    }

    [Fact]
    public void Update_WithEmptyHtmlBody_ShouldThrowException()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Name", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            template.Update("Name", "Subject", "", EmailTemplateCategory.Authentication));
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED, exception.ErrorCode);
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);
        template.Deactivate();

        // Act
        template.Activate();

        // Assert
        Assert.True(template.IsActive);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Act
        template.Deactivate();

        // Assert
        Assert.False(template.IsActive);
        Assert.NotNull(template.UpdatedAt);
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_NonSystemTemplate_ShouldSucceed()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Act
        template.SoftDelete();

        // Assert
        Assert.True(template.IsDeleted);
        Assert.NotNull(template.DeletedAt);
    }

    [Fact]
    public void SoftDelete_SystemTemplate_ShouldThrowException()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "SYSTEM_EMAIL", "System Template", "Subject",
            "<html>Body</html>", EmailTemplateCategory.System,
            isSystem: true);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => template.SoftDelete());
        Assert.Equal(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE, exception.ErrorCode);
    }

    [Fact]
    public void SoftDelete_WithDeletedById_ShouldSucceed()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);
        const long deletedById = 123;

        // Act
        template.SoftDelete(deletedById);

        // Assert
        Assert.True(template.IsDeleted);
        Assert.NotNull(template.DeletedAt);
    }

    #endregion

    #region Category Tests

    [Theory]
    [InlineData(EmailTemplateCategory.Authentication)]
    [InlineData(EmailTemplateCategory.Notification)]
    [InlineData(EmailTemplateCategory.Marketing)]
    [InlineData(EmailTemplateCategory.System)]
    public void Create_WithDifferentCategories_ShouldSucceed(EmailTemplateCategory category)
    {
        // Arrange & Act
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            "<html>Body</html>", category);

        // Assert
        Assert.Equal(category, template.Category);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithVeryLongHtmlBody_ShouldSucceed()
    {
        // Arrange
        var longHtml = new string('x', 100000);

        // Act
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Test", "Subject",
            longHtml, EmailTemplateCategory.Authentication);

        // Assert
        Assert.Equal(longHtml, template.HtmlBody);
    }

    [Fact]
    public void Create_WithSpecialCharactersInCode_ShouldSucceed()
    {
        // Arrange
        const string code = "TEST_EMAIL_2024";

        // Act
        var template = EmailTemplate.Create(
            code, "Test", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Assert
        Assert.Equal(code.ToUpperInvariant(), template.Code);
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtTimestamp()
    {
        // Arrange
        var template = EmailTemplate.Create(
            "TEST_EMAIL", "Original", "Subject",
            "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Act
        template.Update("Updated", "Subject", "<html>Body</html>", EmailTemplateCategory.Authentication);

        // Assert
        Assert.NotNull(template.UpdatedAt);
    }

    #endregion
}
