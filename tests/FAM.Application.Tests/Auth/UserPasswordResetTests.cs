using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FAM.Application.Tests.Auth;

/// <summary>
/// Unit tests cho password reset logic trong User entity
/// </summary>
public class UserPasswordResetTests
{
    [Fact]
    public void SetPasswordResetToken_ShouldSetTokenAndExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "test-reset-token-123";
        var expirationMinutes = 60;

        // Act
        user.SetPasswordResetToken(token, expirationMinutes);

        // Assert
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiresAt.Should().NotBeNull();
        user.PasswordResetTokenExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(expirationMinutes),
            TimeSpan.FromSeconds(5)
        );
    }

    [Fact]
    public void IsPasswordResetTokenValid_ShouldReturnTrue_WhenTokenIsValidAndNotExpired()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "valid-token";
        user.SetPasswordResetToken(token, 60);

        // Act
        var isValid = user.IsPasswordResetTokenValid(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsPasswordResetTokenValid_ShouldReturnFalse_WhenTokenIsExpired()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "expired-token";
        user.SetPasswordResetToken(token, -10); // Set expiration 10 minutes ago

        // Act
        var isValid = user.IsPasswordResetTokenValid(token);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordResetTokenValid_ShouldReturnFalse_WhenTokenDoesNotMatch()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPasswordResetToken("correct-token", 60);

        // Act
        var isValid = user.IsPasswordResetTokenValid("wrong-token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordResetTokenValid_ShouldReturnFalse_WhenNoTokenIsSet()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var isValid = user.IsPasswordResetTokenValid("any-token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ClearPasswordResetToken_ShouldRemoveTokenAndExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPasswordResetToken("some-token", 60);

        // Act
        user.ClearPasswordResetToken();

        // Assert
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordAndClearResetToken()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPasswordResetToken("reset-token", 60);
        
        var oldPasswordHash = user.Password.Hash;
        var newPassword = Password.Create("NewSecurePassword456!");

        // Act
        user.UpdatePassword(newPassword.Hash, newPassword.Salt);

        // Assert
        user.Password.Hash.Should().NotBe(oldPasswordHash);
        user.Password.Hash.Should().Be(newPassword.Hash);
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
        user.PasswordChangedAt.Should().NotBeNull();
        user.PasswordChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void SetPasswordResetToken_ShouldSupportVariousExpirationTimes(int minutes)
    {
        // Arrange
        var user = CreateTestUser();
        var token = $"token-{minutes}min";

        // Act
        user.SetPasswordResetToken(token, minutes);

        // Assert
        user.PasswordResetTokenExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(minutes),
            TimeSpan.FromSeconds(5)
        );
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginInfo()
    {
        // Arrange
        var user = CreateTestUser();
        var ipAddress = "192.168.1.100";

        // Act
        user.RecordLogin(ipAddress);

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastLoginIp.Should().Be(ipAddress);
    }

    private static User CreateTestUser()
    {
        return User.Create(
            username: "testuser",
            email: "test@example.com",
            plainPassword: "Password123!",
            firstName: "Test",
            lastName: "User"
        );
    }
}
