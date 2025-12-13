using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class UsernameTests
{
    [Fact]
    public void Create_WithValidUsername_ShouldCreateUsername()
    {
        // Arrange
        var value = "john_doe123";

        // Act
        var username = Username.Create(value);

        // Assert
        username.Should().NotBeNull();
        username.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => Username.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Username cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        var value = "   ";

        // Act
        Action act = () => Username.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Username cannot be empty");
    }

    [Fact]
    public void Create_WithTooShortUsername_ShouldThrowDomainException()
    {
        // Arrange
        var value = "ab";

        // Act
        Action act = () => Username.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Username must be at least 3 characters long");
    }

    [Fact]
    public void Create_WithTooLongUsername_ShouldThrowDomainException()
    {
        // Arrange
        var value = new string('a', 51);

        // Act
        Action act = () => Username.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Username cannot exceed 50 characters");
    }

    [Fact]
    public void Create_WithInvalidCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var value = "john@doe";

        // Act
        Action act = () => Username.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");
    }

    [Fact]
    public void Create_WithWhitespaceInMiddle_ShouldTrimWhitespace()
    {
        // Arrange
        var value = " john_doe ";

        // Act
        var username = Username.Create(value);

        // Assert
        username.Value.Should().Be("john_doe");
    }

    [Fact]
    public void IsSafeUsername_WithSafeUsername_ShouldReturnTrue()
    {
        // Arrange
        var username = Username.Create("john_doe");

        // Act
        var result = username.IsSafeUsername();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSafeUsername_WithUnsafeUsername_ShouldReturnFalse()
    {
        // Arrange
        var username = Username.Create("admin_user");

        // Act
        var result = username.IsSafeUsername();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeUsername_WithRootUsername_ShouldReturnFalse()
    {
        // Arrange
        var username = Username.Create("myroot");

        // Act
        var result = username.IsSafeUsername();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var username = Username.Create("john_doe");

        // Act
        string value = username;

        // Assert
        value.Should().Be("john_doe");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "john_doe";

        // Act
        var username = Username.Create(value);

        // Assert
        username.Value.Should().Be("john_doe");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var username = Username.Create("john_doe");

        // Act
        var result = username.ToString();

        // Assert
        result.Should().Be("john_doe");
    }
}
