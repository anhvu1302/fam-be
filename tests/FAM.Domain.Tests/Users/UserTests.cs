using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using FluentAssertions;

namespace FAM.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidUsernameAndEmail_ShouldCreateUser()
    {
        // Arrange
        string username = "john_doe";
        string email = "john.doe@example.com";
        string password = "MySecurePass123!";

        // Act
        User user = User.CreateWithPlainPassword(username, email, password);

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().NotBeNull();
        user.Username.Should().Be(username);
        user.Email.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Password.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithUsernameEmailAndFullName_ShouldCreateUserWithFullName()
    {
        // Arrange
        string username = "jane_smith";
        string email = "jane.smith@example.com";
        string password = "MySecurePass123!";
        string firstName = "Jane";
        string lastName = "Smith";

        // Act
        User user = User.CreateWithPlainPassword(username, email, password, firstName, lastName, null);

        // Assert
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void Create_WithNullFullName_ShouldCreateUserWithUsernameAsFullName()
    {
        // Arrange
        string username = "test_user";
        string email = "test@example.com";
        string password = "MySecurePass123!";

        // Act
        User user = User.CreateWithPlainPassword(username, email, password, null, null, null);

        // Assert
        user.FullName.Should().Be(username);
    }

    [Fact]
    public void Create_WithInvalidUsername_ShouldThrowException()
    {
        // Arrange
        string invalidUsername = "";
        string email = "test@example.com";

        // Act & Assert
        Assert.Throws<DomainException>(() => User.CreateWithPlainPassword(invalidUsername, email, "MySecurePass123!"));
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        string username = "test_user";
        string invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<DomainException>(() => User.CreateWithPlainPassword(username, invalidEmail, "MySecurePass123!"));
    }
}
