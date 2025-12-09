using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FluentAssertions;

namespace FAM.Domain.Tests.Authorization;

/// <summary>
/// Unit tests for SigningKey entity
/// </summary>
public class SigningKeyTests
{
    private const string ValidKeyId = "key_123456_abc123";

    private const string ValidPublicKey =
        "-----BEGIN RSA PUBLIC KEY-----\nMIIBCgKCAQEA0Z3US...\n-----END RSA PUBLIC KEY-----";

    private const string ValidPrivateKey =
        "-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEA0Z3US...\n-----END RSA PRIVATE KEY-----";

    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateSigningKey()
    {
        // Act
        var key = SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey,
            "RS256",
            2048,
            DateTime.UtcNow.AddYears(1),
            "Test key");

        // Assert
        key.Should().NotBeNull();
        key.KeyId.Should().Be(ValidKeyId);
        key.PublicKey.Should().Be(ValidPublicKey);
        key.PrivateKey.Should().Be(ValidPrivateKey);
        key.Algorithm.Should().Be("RS256");
        key.KeySize.Should().Be(2048);
        key.Use.Should().Be("sig");
        key.KeyType.Should().Be("RSA");
        key.IsActive.Should().BeTrue();
        key.IsRevoked.Should().BeFalse();
        key.Description.Should().Be("Test key");
    }

    [Fact]
    public void Create_WithoutExpiry_ShouldCreateNonExpiringKey()
    {
        // Act
        var key = SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey);

        // Assert
        key.ExpiresAt.Should().BeNull();
        key.IsExpired().Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidKeyId_ShouldThrowDomainException(string? keyId)
    {
        // Act
        var act = () => SigningKey.Create(
            keyId!,
            ValidPublicKey,
            ValidPrivateKey);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*KeyId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPublicKey_ShouldThrowDomainException(string? publicKey)
    {
        // Act
        var act = () => SigningKey.Create(
            ValidKeyId,
            publicKey!,
            ValidPrivateKey);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*PublicKey*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPrivateKey_ShouldThrowDomainException(string? privateKey)
    {
        // Act
        var act = () => SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            privateKey!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*PrivateKey*");
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Activate_WhenNotRevoked_ShouldSetIsActiveTrue()
    {
        // Arrange
        var key = CreateTestKey();
        key.Deactivate();

        // Act
        key.Activate();

        // Assert
        key.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenRevoked_ShouldThrowDomainException()
    {
        // Arrange
        var key = CreateTestKey();
        key.Revoke("Test revocation");

        // Act
        var act = () => key.Activate();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public void Activate_WhenExpired_ShouldThrowDomainException()
    {
        // Arrange
        var key = SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey,
            expiresAt: DateTime.UtcNow.AddDays(-1));
        key.Deactivate();

        // Act
        var act = () => key.Activate();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var key = CreateTestKey();

        // Act
        key.Deactivate();

        // Assert
        key.IsActive.Should().BeFalse();
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WhenNotRevoked_ShouldRevokeKey()
    {
        // Arrange
        var key = CreateTestKey();
        var reason = "Security breach";

        // Act
        key.Revoke(reason);

        // Assert
        key.IsRevoked.Should().BeTrue();
        key.IsActive.Should().BeFalse();
        key.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        key.RevocationReason.Should().Be(reason);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrowDomainException()
    {
        // Arrange
        var key = CreateTestKey();
        key.Revoke("First revocation");

        // Act
        var act = () => key.Revoke("Second revocation");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already revoked*");
    }

    [Fact]
    public void Revoke_WithoutReason_ShouldRevokeKey()
    {
        // Arrange
        var key = CreateTestKey();

        // Act
        key.Revoke();

        // Assert
        key.IsRevoked.Should().BeTrue();
        key.RevocationReason.Should().BeNull();
    }

    #endregion

    #region CanSign/CanVerify Tests

    [Fact]
    public void CanSign_WhenActiveAndNotRevoked_ShouldReturnTrue()
    {
        // Arrange
        var key = CreateTestKey();

        // Assert
        key.CanSign().Should().BeTrue();
    }

    [Fact]
    public void CanSign_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var key = CreateTestKey();
        key.Deactivate();

        // Assert
        key.CanSign().Should().BeFalse();
    }

    [Fact]
    public void CanSign_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var key = CreateTestKey();
        key.Revoke();

        // Assert
        key.CanSign().Should().BeFalse();
    }

    [Fact]
    public void CanSign_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var key = SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey,
            expiresAt: DateTime.UtcNow.AddDays(-1));

        // Assert
        key.CanSign().Should().BeFalse();
    }

    [Fact]
    public void CanVerify_WhenNotRevoked_ShouldReturnTrue()
    {
        // Arrange
        var key = CreateTestKey();

        // Assert
        key.CanVerify().Should().BeTrue();
    }

    [Fact]
    public void CanVerify_WhenInactiveButNotRevoked_ShouldReturnTrue()
    {
        // Arrange
        var key = CreateTestKey();
        key.Deactivate();

        // Assert - inactive keys can still verify old tokens
        key.CanVerify().Should().BeTrue();
    }

    [Fact]
    public void CanVerify_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var key = CreateTestKey();
        key.Revoke();

        // Assert
        key.CanVerify().Should().BeFalse();
    }

    #endregion

    #region IsExpired Tests

    [Fact]
    public void IsExpired_WhenNoExpiry_ShouldReturnFalse()
    {
        // Arrange
        var key = CreateTestKey();

        // Assert
        key.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiryInFuture_ShouldReturnFalse()
    {
        // Arrange
        var key = SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey,
            expiresAt: DateTime.UtcNow.AddDays(30));

        // Assert
        key.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiryInPast_ShouldReturnTrue()
    {
        // Arrange
        var key = SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey,
            expiresAt: DateTime.UtcNow.AddDays(-1));

        // Assert
        key.IsExpired().Should().BeTrue();
    }

    #endregion

    #region MarkAsUsed Tests

    [Fact]
    public void MarkAsUsed_ShouldUpdateLastUsedAt()
    {
        // Arrange
        var key = CreateTestKey();
        key.LastUsedAt.Should().BeNull();

        // Act
        key.MarkAsUsed();

        // Assert
        key.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Helper Methods

    private static SigningKey CreateTestKey()
    {
        return SigningKey.Create(
            ValidKeyId,
            ValidPublicKey,
            ValidPrivateKey,
            "RS256",
            2048);
    }

    #endregion
}