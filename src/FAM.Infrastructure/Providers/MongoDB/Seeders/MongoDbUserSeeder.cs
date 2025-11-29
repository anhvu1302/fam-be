using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Mongo;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Seeders;

/// <summary>
/// Seeds initial users data for MongoDB
/// </summary>
public class MongoDbUserSeeder : BaseDataSeeder
{
    private readonly MongoDbContext _dbContext;

    public MongoDbUserSeeder(MongoDbContext dbContext, ILogger<MongoDbUserSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140001_MongoDbUserSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed users...");

        var usersCollection = _dbContext.GetCollection<UserMongo>("users");

        // Check if users already exist
        var count = await usersCollection.CountDocumentsAsync(
            FilterDefinition<UserMongo>.Empty,
            cancellationToken: cancellationToken);

        if (count > 0)
        {
            LogInfo("Users already exist, skipping seed");
            return;
        }

        var users = new List<UserMongo>
        {
            new()
            {
                DomainId = 1,
                Username = "admin",
                Email = "admin@fam.local",
                FullName = "System Administrator",
                PasswordHash = "RlI1JkKTEVI6+RhcU/dLzeKshHhDwe3NpWd6Z3BIFtY=", // Hash for Admin@123
                PasswordSalt = "6eb1ccfd64a94810bd3398067dff13f5", // Salt for Admin@123
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await usersCollection.InsertManyAsync(users, cancellationToken: cancellationToken);

        LogInfo($"Seeded {users.Count} users successfully");
    }
}