using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldCreateEmail()
    {
        // Arrange
        string validEmail = "test@example.com";

        // Act
        Email email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithSubdomain_ShouldCreateEmail()
    {
        // Arrange
        string validEmail = "user@subdomain.example.com";

        // Act
        Email email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithNumbers_ShouldCreateEmail()
    {
        // Arrange
        string validEmail = "user123@test456.com";

        // Act
        Email email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithUnderscore_ShouldCreateEmail()
    {
        // Arrange
        string validEmail = "user_name@example.com";

        // Act
        Email email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithDot_ShouldCreateEmail()
    {
        // Arrange
        string validEmail = "user.name@example.com";

        // Act
        Email email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrowDomainException()
    {
        // Arrange
        string? nullEmail = null;

        // Act
        Action act = () => Email.Create(nullEmail!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email is required");
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowDomainException()
    {
        // Arrange
        string emptyEmail = string.Empty;

        // Act
        Action act = () => Email.Create(emptyEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email is required");
    }

    [Fact]
    public void Create_WithWhitespaceEmail_ShouldThrowDomainException()
    {
        // Arrange
        string whitespaceEmail = "   ";

        // Act
        Action act = () => Email.Create(whitespaceEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email is required");
    }

    [Fact]
    public void Create_WithInvalidEmailFormat_ShouldThrowDomainException()
    {
        // Arrange
        string invalidEmail = "not-an-email";

        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid email format");
    }

    [Fact]
    public void Create_WithEmailWithoutAtSymbol_ShouldThrowDomainException()
    {
        // Arrange
        string invalidEmail = "userexample.com";

        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid email format");
    }

    [Fact]
    public void Create_WithEmailWithoutDomain_ShouldThrowDomainException()
    {
        // Arrange
        string invalidEmail = "user@";

        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid email format");
    }

    [Fact]
    public void Create_WithEmailWithMultipleAtSymbols_ShouldThrowDomainException()
    {
        // Arrange
        string invalidEmail = "user@test@example.com";

        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid email format");
    }

    [Fact]
    public void Create_WithEmailWithSpaces_ShouldTrimAndValidate()
    {
        // Arrange
        string emailWithSpaces = "  test@example.com  ";

        // Act
        Email email = Email.Create(emailWithSpaces);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitOperator_ToString_ShouldReturnValue()
    {
        // Arrange
        Email email = Email.Create("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        Email email = Email.Create("test@example.com");

        // Act
        string result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_WithSameEmails_ShouldBeEqual()
    {
        // Arrange
        Email email1 = Email.Create("test@example.com");
        Email email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentEmails_ShouldNotBeEqual()
    {
        // Arrange
        Email email1 = Email.Create("test@example.com");
        Email email2 = Email.Create("other@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void Equality_WithSameEmailDifferentCase_ShouldBeEqual()
    {
        // Arrange
        Email email1 = Email.Create("test@example.com");
        Email email2 = Email.Create("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
    }
}
