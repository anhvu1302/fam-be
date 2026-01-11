using System.Security.Cryptography;

using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Seeders;

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
        bool hasActiveKey = await _dbContext.SigningKeys
            .AnyAsync(k => k.IsActive && !k.IsRevoked && !k.IsDeleted, cancellationToken);

        if (hasActiveKey)
        {
            LogInfo("Active signing key already exists, skipping seed");
            return;
        }

        LogInfo("No active signing key found, generating new RSA key pair...");

        // Generate RSA key pair
        using RSA rsa = RSA.Create(2048);
        string publicKey = rsa.ExportRSAPublicKeyPem();
        string privateKey = rsa.ExportRSAPrivateKeyPem();
        string keyId = GenerateKeyId();

        SigningKey signingKey = SigningKey.Create(
            keyId,
            publicKey,
            privateKey,
            "RS256",
            2048,
            null,
            "Initial signing key generated during system setup"
        );

        await _dbContext.SigningKeys.AddAsync(signingKey, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created initial signing key with ID: {keyId}");
    }

    private static string GenerateKeyId()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        byte[] randomBytes = new byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        string randomPart = Convert.ToHexString(randomBytes).ToLowerInvariant();
        return $"key_{timestamp}_{randomPart}";
    }
}
