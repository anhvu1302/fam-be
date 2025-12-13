using System.Security.Cryptography;

using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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

    public async Task<JwksDto> GetJwksAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SigningKey> keys = await _repository.GetAllActiveKeysAsync(cancellationToken);
        var jwks = new JwksDto();

        foreach (SigningKey key in keys.Where(k => k.CanVerify()))
            try
            {
                JwkDto jwk = ConvertToJwk(key);
                jwks.Keys.Add(jwk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert key {KeyId} to JWK", key.KeyId);
            }

        return jwks;
    }

    public async Task<SigningKey> GetOrCreateActiveKeyAsync(CancellationToken cancellationToken = default)
    {
        SigningKey? activeKey = await _repository.GetActiveKeyAsync(cancellationToken);

        if (activeKey != null && activeKey.CanSign()) return activeKey;

        _logger.LogInformation("No active signing key found, generating new key");
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
        var validAlgorithms = new[] { "RS256", "RS384", "RS512" };
        if (!validAlgorithms.Contains(algorithm))
            throw new DomainException(ErrorCodes.KEY_INVALID_ALGORITHM);

        // Validate key size
        var validKeySizes = new[] { 2048, 3072, 4096 };
        if (!validKeySizes.Contains(keySize))
            throw new DomainException(ErrorCodes.KEY_INVALID_SIZE);

        // Generate RSA key pair
        using var rsa = RSA.Create(keySize);
        var publicKey = rsa.ExportRSAPublicKeyPem();
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        var keyId = GenerateKeyId();

        var signingKey = SigningKey.Create(
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

        _logger.LogInformation(
            "Generated new signing key {KeyId} with algorithm {Algorithm} and size {KeySize}",
            keyId, algorithm, keySize);

        return signingKey;
    }

    public async Task ActivateKeyAsync(long keyId, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null) throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);

        key.Activate();
        _repository.Update(key);

        // Deactivate all other keys
        await _repository.DeactivateAllExceptAsync(keyId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated signing key {KeyId}", key.KeyId);
    }

    public async Task DeactivateKeyAsync(long keyId, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null) throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);

        key.Deactivate();
        _repository.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated signing key {KeyId}", key.KeyId);
    }

    public async Task RevokeKeyAsync(long keyId, string? reason = null, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null) throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);

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
                oldKey.Revoke(revocationReason ?? "Key rotation");
            else
                oldKey.Deactivate();
            _repository.Update(oldKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Rotated signing keys. New key: {NewKeyId}, Old key: {OldKeyId} ({Action})",
            newKey.KeyId,
            oldKey?.KeyId ?? "none",
            revokeOldKey ? "revoked" : "deactivated");

        return newKey;
    }

    public async Task<IReadOnlyList<SigningKeyResponse>> GetAllKeysAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<SigningKey> keys = await _repository.GetAllAsync(cancellationToken);
        return keys.Select(MapToResponse).ToList();
    }

    public async Task<SigningKeyResponse?> GetKeyByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(id, cancellationToken);
        return key != null ? MapToResponse(key) : null;
    }

    public async Task<SigningKey?> GetKeyByKeyIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByKeyIdAsync(keyId, cancellationToken);
    }

    public async Task DeleteKeyAsync(long keyId, CancellationToken cancellationToken = default)
    {
        SigningKey? key = await _repository.GetByIdAsync(keyId, cancellationToken);
        if (key == null) throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", keyId);

        if (!key.IsRevoked) throw new DomainException(ErrorCodes.KEY_MUST_REVOKE_FIRST);

        _repository.Delete(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted signing key {KeyId}", key.KeyId);
    }

    public async Task<IReadOnlyList<SigningKeyResponse>> GetExpiringKeysAsync(
        TimeSpan withinTimeSpan,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SigningKey>
            keys = await _repository.GetKeysExpiringWithinAsync(withinTimeSpan, cancellationToken);
        return keys.Select(MapToResponse).ToList();
    }

    #region Helper Methods

    private static string GenerateKeyId()
    {
        // Generate a unique key ID using timestamp and random bytes
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var randomBytes = new byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        var randomPart = Convert.ToHexString(randomBytes).ToLowerInvariant();
        return $"key_{timestamp}_{randomPart}";
    }

    private static JwkDto ConvertToJwk(SigningKey key)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(key.PublicKey);
        RSAParameters parameters = rsa.ExportParameters(false);

        return new JwkDto
        {
            Kty = "RSA",
            Use = "sig",
            Kid = key.KeyId,
            Alg = key.Algorithm,
            N = Base64UrlEncoder.Encode(parameters.Modulus!),
            E = Base64UrlEncoder.Encode(parameters.Exponent!)
        };
    }

    private static SigningKeyResponse MapToResponse(SigningKey key)
    {
        return new SigningKeyResponse
        {
            Id = key.Id,
            KeyId = key.KeyId,
            Algorithm = key.Algorithm,
            KeySize = key.KeySize,
            IsActive = key.IsActive,
            IsRevoked = key.IsRevoked,
            RevokedAt = key.RevokedAt,
            RevocationReason = key.RevocationReason,
            ExpiresAt = key.ExpiresAt,
            LastUsedAt = key.LastUsedAt,
            Description = key.Description,
            CreatedAt = key.CreatedAt,
            IsExpired = key.IsExpired(),
            CanSign = key.CanSign(),
            CanVerify = key.CanVerify()
        };
    }

    #endregion
}
