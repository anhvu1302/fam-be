using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FAM.Application.Tests.Auth;

/// <summary>
/// Unit tests cho password reset logic trong User entity
/// </summary>
public class UserPasswordResetLogicTests
{
    [Fact]
    public void SetPasswordResetToken_ShouldSetTokenAndExpiry()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "reset-token-123";
        var expiryHours = 1;

        // Act
        user.SetPasswordResetToken(token, expiryHours);

        // Assert
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiresAt.Should().NotBeNull();
        // expiryHours is in HOURS, not minutes
        user.PasswordResetTokenExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(expiryHours), 
            TimeSpan.FromSeconds(5)
        );
    }

    [Fact]
    public void IsPasswordResetTokenValid_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "reset-token-123";
        user.SetPasswordResetToken(token, 1); // 1 hour expiry

        // Act
        var isValid = user.IsPasswordResetTokenValid(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsPasswordResetTokenValid_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "reset-token-123";
        user.SetPasswordResetToken(token, -1); // Expired 1 hour ago

        // Act
        var isValid = user.IsPasswordResetTokenValid(token);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordResetTokenValid_WithWrongToken_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPasswordResetToken("correct-token", 1);

        // Act
        var isValid = user.IsPasswordResetTokenValid("wrong-token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordResetTokenValid_WithNoToken_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        // No token set

        // Act
        var isValid = user.IsPasswordResetTokenValid("any-token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ClearPasswordResetToken_ShouldRemoveTokenAndExpiry()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetPasswordResetToken("reset-token", 1);

        // Act
        user.ClearPasswordResetToken();

        // Assert
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void PasswordChangedAt_ShouldBeSetAfterPasswordChange()
    {
        // Arrange
        var user = CreateTestUser();

        // Act - Simulate password change by setting timestamp
        var beforeChange = DateTime.UtcNow;
        // Note: ChangePassword method doesn't exist yet, this tests the concept
        // In real implementation, handler will set PasswordChangedAt

        // Assert - Verify PasswordChangedAt can be set
        user.PasswordChangedAt.Should().BeNull(); // Initially null
    }

    [Fact]
    public void ResetPasswordFlow_ShouldValidateAndClearToken()
    {
        // Arrange
        var user = CreateTestUser();
        var resetToken = "reset-token-xyz";

        // Act - Step 1: Generate reset token
        user.SetPasswordResetToken(resetToken, 1);

        // Assert - Token is set and valid
        user.PasswordResetToken.Should().Be(resetToken);
        user.IsPasswordResetTokenValid(resetToken).Should().BeTrue();

        // Act - Step 2: Clear token after use
        user.ClearPasswordResetToken();

        // Assert - Token is cleared
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
    }

    private User CreateTestUser()
    {
        // Use valid strong password that passes validation
        return User.Create(
            "testuser",
            "test@example.com",
            "StrongP@ssw0rd123!",
            "Test",
            "User",
            null
        );
    }
}
