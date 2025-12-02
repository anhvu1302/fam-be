using System.Security.Cryptography;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial RSA signing key for JWT tokens
/// This ensures there's always a signing key available for token generation
/// </summary>
public class SigningKeySeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public SigningKeySeeder(PostgreSqlDbContext dbContext, ILogger<SigningKeySeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140000_SigningKeySeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing signing keys...");

        // Check if an active signing key already exists
        var hasActiveKey = await _dbContext.SigningKeys
            .AnyAsync(k => k.IsActive && !k.IsRevoked && !k.IsDeleted, cancellationToken);

        if (hasActiveKey)
        {
            LogInfo("Active signing key already exists, skipping seed");
            return;
        }

        LogInfo("No active signing key found, generating new RSA key pair...");

        // Generate RSA key pair
        using var rsa = RSA.Create(2048);
        var publicKey = rsa.ExportRSAPublicKeyPem();
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        var keyId = GenerateKeyId();

        var signingKey = new SigningKeyEf
        {
            KeyId = keyId,
            PublicKey = publicKey,
            PrivateKey = privateKey,
            Algorithm = "RS256",
            KeySize = 2048,
            Use = "sig",
            KeyType = "RSA",
            IsActive = true,
            IsRevoked = false,
            Description = "Initial signing key generated during system setup",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.SigningKeys.AddAsync(signingKey, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created initial signing key with ID: {keyId}");
    }

    private static string GenerateKeyId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var randomBytes = new byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        var randomPart = Convert.ToHexString(randomBytes).ToLowerInvariant();
        return $"key_{timestamp}_{randomPart}";
    }
}