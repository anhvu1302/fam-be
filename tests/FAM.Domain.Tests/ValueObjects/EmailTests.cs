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
        var validEmail = "test@example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithSubdomain_ShouldCreateEmail()
    {
        // Arrange
        var validEmail = "user@subdomain.example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithNumbers_ShouldCreateEmail()
    {
        // Arrange
        var validEmail = "user123@test456.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithUnderscore_ShouldCreateEmail()
    {
        // Arrange
        var validEmail = "user_name@example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithValidEmailWithDot_ShouldCreateEmail()
    {
        // Arrange
        var validEmail = "user.name@example.com";

        // Act
        var email = Email.Create(validEmail);

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
        var emptyEmail = string.Empty;

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
        var whitespaceEmail = "   ";

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
        var invalidEmail = "not-an-email";

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
        var invalidEmail = "userexample.com";

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
        var invalidEmail = "user@";

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
        var invalidEmail = "user@test@example.com";

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
        var emailWithSpaces = "  test@example.com  ";

        // Act
        var email = Email.Create(emailWithSpaces);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitOperator_ToString_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_WithSameEmails_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentEmails_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("other@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void Equality_WithSameEmailDifferentCase_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
    }
}
