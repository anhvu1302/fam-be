using System.Security.Cryptography;

using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Mongo;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Seeders;

/// <summary>
/// Seeds initial RSA signing key for JWT tokens in MongoDB
/// </summary>
public class MongoDbSigningKeySeeder : BaseDataSeeder
{
    private readonly MongoDbContext _context;

    public MongoDbSigningKeySeeder(MongoDbContext context, ILogger<MongoDbSigningKeySeeder> logger)
        : base(logger)
    {
        _context = context;
    }

    public override string Name => "20251129140000_MongoDbSigningKeySeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing signing keys...");

        IMongoCollection<SigningKeyMongo> collection = _context.GetCollection<SigningKeyMongo>("signingKeys");

        // Check if an active signing key already exists
        var hasActiveKey = await collection
            .Find(k => k.IsActive && !k.IsRevoked && !k.IsDeleted)
            .AnyAsync(cancellationToken);

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

        var signingKey = new SigningKeyMongo
        {
            DomainId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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

        await collection.InsertOneAsync(signingKey, cancellationToken: cancellationToken);

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
