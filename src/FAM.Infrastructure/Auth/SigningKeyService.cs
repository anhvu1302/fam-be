using System.Security.Cryptography;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Auth;

/// <summary>
/// Service for managing RSA signing keys for JWT
/// </summary>
public class SigningKeyService : ISigningKeyService
{
    private readonly ISigningKeyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SigningKeyService> _logger;

    public SigningKeyService(
        ISigningKeyRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<SigningKeyService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SigningKey> GetOrCreateActiveKeyAsync(CancellationToken cancellationToken = default)
    {
        SigningKey? activeKey = await _repository.GetActiveKeyAsync(cancellationToken);

        if (activeKey != null && activeKey.CanSign())
        {
            return activeKey;
        }

        return await GenerateKeyAsync(cancellationToken: cancellationToken);
    }

    public async Task<SigningKey> GenerateKeyAsync(
        string algorithm = "RS256",
        int keySize = 2048,
        DateTime? expiresAt = null,
        string? description = null,
        bool activateImmediately = true,
        CancellationToken cancellationToken = default)
    {
        // Validate algorithm
        string[] validAlgorithms = new[] { "RS256", "RS384", "RS512" };
        if (!validAlgorithms.Contains(algorithm))
        {
            throw new DomainException(ErrorCodes.KEY_INVALID_ALGORITHM);
        }

        // Validate key size
        int[] validKeySizes = new[] { 2048, 3072, 4096 };
        if (!validKeySizes.Contains(keySize))
        {
            throw new DomainException(ErrorCodes.KEY_INVALID_SIZE);
        }

        // Generate RSA key pair
        using RSA rsa = RSA.Create(keySize);
        string publicKey = rsa.ExportRSAPublicKeyPem();
        string privateKey = rsa.ExportRSAPrivateKeyPem();
        string keyId = GenerateKeyId();

        SigningKey signingKey = SigningKey.Create(
            keyId,
            publicKey,
            privateKey,
            algorithm,
            keySize,
            expiresAt,
            description ?? $"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

        await _repository.AddAsync(signingKey, cancellationToken);

        if (activateImmediately)
        {
            // Deactivate all other keys
            IEnumerable<SigningKey> allKeys = await _repository.GetAllAsync(cancellationToken);
            foreach (SigningKey key in allKeys.Where(k => k.Id != signingKey.Id && k.IsActive))
            {
                key.Deactivate();
                _repository.Update(key);
            }
        }
        else
        {
            signingKey.Deactivate();
            _repository.Update(signingKey);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return signingKey;
    }

    public async Task ActivateKeyAsync(long keyId, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null)
        {
            throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);
        }

        key.Activate();
        _repository.Update(key);

        // Deactivate all other keys
        await _repository.DeactivateAllExceptAsync(keyId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateKeyAsync(long keyId, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null)
        {
            throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);
        }

        key.Deactivate();
        _repository.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeKeyAsync(long keyId, string? reason = null, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null)
        {
            throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);
        }

        key.Revoke(reason);
        _repository.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Revoked signing key {KeyId}. Reason: {Reason}", key.KeyId, reason ?? "Not specified");
    }

    public async Task<SigningKey> RotateKeyAsync(
        string algorithm = "RS256",
        int keySize = 2048,
        DateTime? expiresAt = null,
        string? description = null,
        bool revokeOldKey = false,
        string? revocationReason = null,
        CancellationToken cancellationToken = default)
    {
        SigningKey? oldKey = await _repository.GetActiveKeyAsync(cancellationToken);

        // Generate new key
        SigningKey newKey = await GenerateKeyAsync(
            algorithm,
            keySize,
            expiresAt,
            description ?? "Key rotation",
            true,
            cancellationToken);

        // Handle old key
        if (oldKey != null)
        {
            if (revokeOldKey)
            {
                oldKey.Revoke(revocationReason ?? "Key rotation");
            }
            else
            {
                oldKey.Deactivate();
            }

            _repository.Update(oldKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }


        return newKey;
    }

    public async Task<IReadOnlyList<SigningKey>> GetAllKeysAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<SigningKey> keys = await _repository.GetAllAsync(cancellationToken);
        return keys.ToList();
    }

    public async Task<SigningKey?> GetKeyByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<SigningKey?> GetKeyByKeyIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByKeyIdAsync(keyId, cancellationToken);
    }

    public async Task DeleteKeyAsync(long keyId, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null)
        {
            throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);
        }

        if (!key.IsRevoked)
        {
            throw new DomainException(ErrorCodes.KEY_MUST_REVOKE_FIRST);
        }

        _repository.Delete(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SigningKey>> GetExpiringKeysAsync(
        TimeSpan withinTimeSpan,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetKeysExpiringWithinAsync(withinTimeSpan, cancellationToken);
    }

    #region Helper Methods

    private static string GenerateKeyId()
    {
        // Generate a unique key ID using timestamp and random bytes
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        byte[] randomBytes = new byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        string randomPart = Convert.ToHexString(randomBytes).ToLowerInvariant();
        return $"key_{timestamp}_{randomPart}";
    }

    #endregion
}
