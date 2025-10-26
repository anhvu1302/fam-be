using FluentAssertions;
using FAM.Domain.Common;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using Xunit;

namespace FAM.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidUsernameAndEmail_ShouldCreateUser()
    {
        // Arrange
        var username = "john_doe";
        var email = "john.doe@example.com";

        // Act
        var user = User.Create(username, email);

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().NotBeNull();
        user.Username.Value.Should().Be(username);
        user.Email.Should().NotBeNull();
        user.Email.Value.Should().Be(email);
    }

    [Fact]
    public void Create_WithUsernameEmailAndFullName_ShouldCreateUserWithFullName()
    {
        // Arrange
        var username = "jane_smith";
        var email = "jane.smith@example.com";
        var fullName = "Jane Smith";

        // Act
        var user = User.Create(username, email, fullName);

        // Assert
        user.Username.Value.Should().Be(username);
        user.Email.Value.Should().Be(email);
        user.FullName.Should().Be(fullName);
    }

    [Fact]
    public void Create_WithNullFullName_ShouldCreateUserWithNullFullName()
    {
        // Arrange
        var username = "test_user";
        var email = "test@example.com";

        // Act
        var user = User.Create(username, email, null);

        // Assert
        user.FullName.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidUsername_ShouldThrowException()
    {
        // Arrange
        var invalidUsername = "";
        var email = "test@example.com";

        // Act & Assert
        Assert.Throws<DomainException>(() => User.Create(invalidUsername, email));
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var username = "test_user";
        var invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<DomainException>(() => User.Create(username, invalidEmail));
    }
}