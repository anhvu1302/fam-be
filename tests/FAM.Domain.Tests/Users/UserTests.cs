using FAM.Domain.Common;
using FAM.Domain.Users;
using FluentAssertions;

namespace FAM.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidUsernameAndEmail_ShouldCreateUser()
    {
        // Arrange
        var username = "john_doe";
        var email = "john.doe@example.com";
        var password = "MySecurePass123!";

        // Act
        var user = User.Create(username, email, password);

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().NotBeNull();
        user.Username.Value.Should().Be(username);
        user.Email.Should().NotBeNull();
        user.Email.Value.Should().Be(email);
        user.Password.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithUsernameEmailAndFullName_ShouldCreateUserWithFullName()
    {
        // Arrange
        var username = "jane_smith";
        var email = "jane.smith@example.com";
        var password = "MySecurePass123!";
        var firstName = "Jane";
        var lastName = "Smith";

        // Act
        var user = User.Create(username, email, password, firstName, lastName, null);

        // Assert
        user.Username.Value.Should().Be(username);
        user.Email.Value.Should().Be(email);
        user.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void Create_WithNullFullName_ShouldCreateUserWithUsernameAsFullName()
    {
        // Arrange
        var username = "test_user";
        var email = "test@example.com";
        var password = "MySecurePass123!";

        // Act
        var user = User.Create(username, email, password, null, null, null);

        // Assert
        user.FullName.Should().Be(username);
    }

    [Fact]
    public void Create_WithInvalidUsername_ShouldThrowException()
    {
        // Arrange
        var invalidUsername = "";
        var email = "test@example.com";

        // Act & Assert
        Assert.Throws<DomainException>(() => User.Create(invalidUsername, email, "MySecurePass123!"));
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var username = "test_user";
        var invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<DomainException>(() => User.Create(username, invalidEmail, "MySecurePass123!"));
    }
}