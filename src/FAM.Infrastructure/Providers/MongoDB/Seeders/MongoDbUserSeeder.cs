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

    public override int Order => 1;

    public override string Name => "MongoDB User Seeder";

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
            new UserMongo
            {
                DomainId = 1,
                Username = "admin",
                Email = "admin@fam.local",
                FullName = "System Administrator",
                PasswordHash = "", // TODO: Set proper password hash
                PasswordSalt = "", // TODO: Set proper password salt
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserMongo
            {
                DomainId = 2,
                Username = "manager",
                Email = "manager@fam.local",
                FullName = "Asset Manager",
                PasswordHash = "", // TODO: Set proper password hash
                PasswordSalt = "", // TODO: Set proper password salt
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserMongo
            {
                DomainId = 3,
                Username = "user",
                Email = "user@fam.local",
                FullName = "Regular User",
                PasswordHash = "", // TODO: Set proper password hash
                PasswordSalt = "", // TODO: Set proper password salt
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await usersCollection.InsertManyAsync(users, cancellationToken: cancellationToken);

        LogInfo($"Seeded {users.Count} users successfully");
    }
}
