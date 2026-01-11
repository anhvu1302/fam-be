using System.Reflection;
using System.Security.Cryptography;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Infrastructure.Auth;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

namespace FAM.Application.Tests.Auth;

/// <summary>
/// Unit tests for SigningKeyService
/// </summary>
public class SigningKeyServiceTests
{
    private readonly Mock<ISigningKeyRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<SigningKeyService>> _mockLogger;
    private readonly SigningKeyService _service;

    public SigningKeyServiceTests()
    {
        _mockRepository = new Mock<ISigningKeyRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<SigningKeyService>>();

        _mockUnitOfWork.Setup(u => u.SigningKeys).Returns(_mockRepository.Object);

        _service = new SigningKeyService(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    #region GetOrCreateActiveKeyAsync Tests

    [Fact]
    public async Task GetOrCreateActiveKeyAsync_WithExistingActiveKey_ShouldReturnExistingKey()
    {
        // Arrange
        SigningKey existingKey = CreateTestSigningKey("existing_key");
        _mockRepository
            .Setup(r => r.GetActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingKey);

        // Act
        SigningKey result = await _service.GetOrCreateActiveKeyAsync();

        // Assert
        result.Should().Be(existingKey);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateActiveKeyAsync_WithNoActiveKey_ShouldGenerateNewKey()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey>());

        // Act
        SigningKey result = await _service.GetOrCreateActiveKeyAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GenerateKeyAsync Tests

    [Fact]
    public async Task GenerateKeyAsync_WithValidParameters_ShouldGenerateKey()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey>());

        // Act
        SigningKey result = await _service.GenerateKeyAsync(
            "RS256",
            2048,
            description: "Test key");

        // Assert
        result.Should().NotBeNull();
        result.Algorithm.Should().Be("RS256");
        result.KeySize.Should().Be(2048);
        result.Description.Should().Contain("Test key");
        result.IsActive.Should().BeTrue();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("RS256")]
    [InlineData("RS384")]
    [InlineData("RS512")]
    public async Task GenerateKeyAsync_WithValidAlgorithms_ShouldSucceed(string algorithm)
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey>());

        // Act
        SigningKey result = await _service.GenerateKeyAsync(algorithm);

        // Assert
        result.Algorithm.Should().Be(algorithm);
    }

    [Fact]
    public async Task GenerateKeyAsync_WithInvalidAlgorithm_ShouldThrowArgumentException()
    {
        // Act
        Func<Task<SigningKey>> act = async () => await _service.GenerateKeyAsync("INVALID");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*algorithm*");
    }

    [Theory]
    [InlineData(2048)]
    [InlineData(3072)]
    [InlineData(4096)]
    public async Task GenerateKeyAsync_WithValidKeySizes_ShouldSucceed(int keySize)
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey>());

        // Act
        SigningKey result = await _service.GenerateKeyAsync(keySize: keySize);

        // Assert
        result.KeySize.Should().Be(keySize);
    }

    [Fact]
    public async Task GenerateKeyAsync_WithInvalidKeySize_ShouldThrowArgumentException()
    {
        // Act
        Func<Task<SigningKey>> act = async () => await _service.GenerateKeyAsync(keySize: 1024);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*key size*");
    }

    [Fact]
    public async Task GenerateKeyAsync_WithActivateImmediately_ShouldDeactivateOtherActiveKeys()
    {
        // Arrange
        // Create existing key and set its Id via reflection to simulate DB-assigned Id
        SigningKey existingKey = CreateTestSigningKey("existing");
        SetPrivateProperty(existingKey, "Id", 999L); // Simulate existing key in DB with Id != 0

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey> { existingKey });

        // Act
        await _service.GenerateKeyAsync(activateImmediately: true);

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<SigningKey>(k => k.KeyId == "existing")), Times.Once);
    }

    private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
    {
        PropertyInfo? property = obj.GetType().GetProperty(propertyName);
        property?.SetValue(obj, value);
    }

    [Fact]
    public async Task GenerateKeyAsync_WithoutActivateImmediately_ShouldNotActivateNewKey()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey>());

        // Act
        SigningKey result = await _service.GenerateKeyAsync(activateImmediately: false);

        // Assert
        result.IsActive.Should().BeFalse();
    }

    #endregion

    #region ActivateKeyAsync Tests

    [Fact]
    public async Task ActivateKeyAsync_WithValidKey_ShouldActivateKey()
    {
        // Arrange
        SigningKey key = CreateTestSigningKey("test_key");
        key.Deactivate();

        _mockRepository
            .Setup(r => r.GetByIdAsync(1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);

        // Act
        await _service.ActivateKeyAsync(1L);

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<SigningKey>(k => k.IsActive)), Times.Once);
        _mockRepository.Verify(r => r.DeactivateAllExceptAsync(1L, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateKeyAsync_WithNonExistentKey_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999L, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        Func<Task> act = async () => await _service.ActivateKeyAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeactivateKeyAsync Tests

    [Fact]
    public async Task DeactivateKeyAsync_WithValidKey_ShouldDeactivateKey()
    {
        // Arrange
        SigningKey key = CreateTestSigningKey("test_key");
        _mockRepository
            .Setup(r => r.GetByIdAsync(1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);

        // Act
        await _service.DeactivateKeyAsync(1L);

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<SigningKey>(k => !k.IsActive)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateKeyAsync_WithNonExistentKey_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999L, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        Func<Task> act = async () => await _service.DeactivateKeyAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RevokeKeyAsync Tests

    [Fact]
    public async Task RevokeKeyAsync_WithValidKey_ShouldRevokeKey()
    {
        // Arrange
        SigningKey key = CreateTestSigningKey("test_key");
        string reason = "Security breach";

        _mockRepository
            .Setup(r => r.GetByIdAsync(1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);

        // Act
        await _service.RevokeKeyAsync(1L, reason);

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<SigningKey>(k => k.IsRevoked && k.RevocationReason == reason)),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeKeyAsync_WithNonExistentKey_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999L, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        Func<Task> act = async () => await _service.RevokeKeyAsync(999L);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RotateKeyAsync Tests

    [Fact]
    public async Task RotateKeyAsync_WithExistingActiveKey_ShouldRotateKeys()
    {
        // Arrange
        SigningKey oldKey = CreateTestSigningKey("old_key");

        _mockRepository
            .Setup(r => r.GetActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldKey);

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey> { oldKey });

        // Act
        SigningKey result = await _service.RotateKeyAsync(revokeOldKey: false);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        _mockRepository.Verify(r => r.Update(It.Is<SigningKey>(k => k.KeyId == "old_key" && !k.IsActive)),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RotateKeyAsync_WithRevokeOldKey_ShouldRevokeOldKey()
    {
        // Arrange
        SigningKey oldKey = CreateTestSigningKey("old_key");

        _mockRepository
            .Setup(r => r.GetActiveKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldKey);

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey> { oldKey });

        // Act
        await _service.RotateKeyAsync(revokeOldKey: true, revocationReason: "Key rotation");

        // Assert
        _mockRepository.Verify(r => r.Update(It.Is<SigningKey>(k => k.KeyId == "old_key" && k.IsRevoked)),
            Times.AtLeastOnce);
    }

    #endregion

    #region DeleteKeyAsync Tests

    [Fact]
    public async Task DeleteKeyAsync_WithRevokedKey_ShouldDeleteKey()
    {
        // Arrange
        SigningKey key = CreateTestSigningKey("test_key");
        key.Revoke("Test");

        _mockRepository
            .Setup(r => r.GetByIdAsync(1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);

        // Act
        await _service.DeleteKeyAsync(1L);

        // Assert
        _mockRepository.Verify(r => r.Delete(key), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteKeyAsync_WithNonRevokedKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        SigningKey key = CreateTestSigningKey("test_key"); // Not revoked

        _mockRepository
            .Setup(r => r.GetByIdAsync(1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);

        // Act
        Func<Task> act = async () => await _service.DeleteKeyAsync(1L);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*revoke the key before deleting*");
    }

    [Fact]
    public async Task DeleteKeyAsync_WithNonExistentKey_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999L, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        Func<Task> act = async () => await _service.DeleteKeyAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAllKeysAsync Tests

    [Fact]
    public async Task GetAllKeysAsync_ShouldReturnAllKeys()
    {
        // Arrange
        List<SigningKey> keys = new()
        {
            CreateTestSigningKey("key1"),
            CreateTestSigningKey("key2"),
            CreateTestSigningKey("key3")
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(keys);

        // Act
        IReadOnlyList<SigningKey> result = await _service.GetAllKeysAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(r =>
        {
            r.Id.Should().BeGreaterThanOrEqualTo(0);
            r.KeyId.Should().NotBeNullOrEmpty();
            r.Algorithm.Should().Be("RS256");
        });
    }

    #endregion

    #region GetExpiringKeysAsync Tests

    [Fact]
    public async Task GetExpiringKeysAsync_ShouldReturnExpiringKeys()
    {
        // Arrange
        SigningKey expiringKey = CreateTestSigningKey("expiring_key", DateTime.UtcNow.AddDays(5));

        _mockRepository
            .Setup(r => r.GetKeysExpiringWithinAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SigningKey> { expiringKey });

        // Act
        IReadOnlyList<SigningKey> result = await _service.GetExpiringKeysAsync(TimeSpan.FromDays(7));

        // Assert
        result.Should().HaveCount(1);
        result[0].KeyId.Should().Be("expiring_key");
    }

    #endregion

    #region Helper Methods

    private static SigningKey CreateTestSigningKey(string keyId, DateTime? expiresAt = null)
    {
        using RSA rsa = RSA.Create(2048);
        string publicKey = rsa.ExportRSAPublicKeyPem();
        string privateKey = rsa.ExportRSAPrivateKeyPem();

        return SigningKey.Create(
            keyId,
            publicKey,
            privateKey,
            "RS256",
            2048,
            expiresAt);
    }

    #endregion
}
