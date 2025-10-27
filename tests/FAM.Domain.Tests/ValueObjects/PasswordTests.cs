using FluentAssertions;
using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using Xunit;

namespace FAM.Domain.Tests.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldCreatePassword()
    {
        // Arrange
        var plainPassword = "MySecurePass123!";

        // Act
        var password = Password.Create(plainPassword);

        // Assert
        password.Should().NotBeNull();
        password.Hash.Should().NotBeNullOrEmpty();
        password.Salt.Should().NotBeNullOrEmpty();
        password.Verify(plainPassword).Should().BeTrue();
    }

    [Fact]
    public void Create_WithPasswordTooShort_ShouldThrowException()
    {
        // Arrange
        var shortPassword = "12345";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(shortPassword))
            .Message.Should().Contain("at least 8 characters");
    }

    [Fact]
    public void Create_WithPasswordWithoutUppercase_ShouldThrowException()
    {
        // Arrange
        var passwordWithoutUppercase = "mysecurepass123!";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(passwordWithoutUppercase))
            .Message.Should().Contain("uppercase letter");
    }

    [Fact]
    public void Create_WithPasswordWithoutLowercase_ShouldThrowException()
    {
        // Arrange
        var passwordWithoutLowercase = "MYSECUREPASS123!";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(passwordWithoutLowercase))
            .Message.Should().Contain("lowercase letter");
    }

    [Fact]
    public void Create_WithPasswordWithoutNumber_ShouldThrowException()
    {
        // Arrange
        var passwordWithoutNumber = "MySecurePass!";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(passwordWithoutNumber))
            .Message.Should().Contain("number");
    }

    [Fact]
    public void Create_WithPasswordWithoutSpecialCharacter_ShouldThrowException()
    {
        // Arrange
        var passwordWithoutSpecialChar = "MySecurePass123";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(passwordWithoutSpecialChar))
            .Message.Should().Contain("special character");
    }

    [Fact]
    public void Create_WithEmptyPassword_ShouldThrowException()
    {
        // Arrange
        var emptyPassword = "";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(emptyPassword))
            .Message.Should().Contain("cannot be empty");
    }

    [Fact]
    public void Create_WithNullPassword_ShouldThrowException()
    {
        // Arrange
        string? nullPassword = null;

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.Create(nullPassword!))
            .Message.Should().Contain("cannot be empty");
    }

    [Fact]
    public void FromHash_WithValidHashAndSalt_ShouldCreatePassword()
    {
        // Arrange
        var hash = "somehash";
        var salt = "somesalt";

        // Act
        var password = Password.FromHash(hash, salt);

        // Assert
        password.Should().NotBeNull();
        password.Hash.Should().Be(hash);
        password.Salt.Should().Be(salt);
    }

    [Fact]
    public void FromHash_WithEmptyHash_ShouldThrowException()
    {
        // Arrange
        var emptyHash = "";
        var salt = "somesalt";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.FromHash(emptyHash, salt))
            .Message.Should().Contain("cannot be empty");
    }

    [Fact]
    public void FromHash_WithEmptySalt_ShouldThrowException()
    {
        // Arrange
        var hash = "somehash";
        var emptySalt = "";

        // Act & Assert
        Assert.Throws<DomainException>(() => Password.FromHash(hash, emptySalt))
            .Message.Should().Contain("cannot be empty");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var plainPassword = "MySecurePass123!";
        var password = Password.Create(plainPassword);

        // Act
        var result = password.Verify(plainPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var plainPassword = "MySecurePass123!";
        var wrongPassword = "WrongPassword123!";
        var password = Password.Create(plainPassword);

        // Act
        var result = password.Verify(wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Update_WithNewPassword_ShouldUpdatePassword()
    {
        // Arrange
        var oldPassword = "MySecurePass123!";
        var newPassword = "NewSecurePass456!";
        var password = Password.Create(oldPassword);

        // Act
        var updatedPassword = password.Update(newPassword);

        // Assert
        updatedPassword.Should().NotBeNull();
        updatedPassword.Verify(newPassword).Should().BeTrue();
        updatedPassword.Verify(oldPassword).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnMaskedString()
    {
        // Arrange
        var plainPassword = "MySecurePass123!";
        var password = Password.Create(plainPassword);

        // Act
        var result = password.ToString();

        // Assert
        result.Should().Be("*****");
    }
}