using FAM.Infrastructure.PersistenceModels.Mongo;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB;

/// <summary>
/// MongoDB class maps and conventions setup
/// </summary>
public static class MongoClassMaps
{
    public static void Register()
    {
        // Register class maps
        if (!BsonClassMap.IsClassMapRegistered(typeof(CompanyMongo)))
        {
            BsonClassMap.RegisterClassMap<CompanyMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserMongo)))
        {
            BsonClassMap.RegisterClassMap<UserMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}

/// <summary>
/// MongoDB indexes setup
/// </summary>
public static class MongoIndexes
{
    public static async Task CreateIndexesAsync(IMongoDatabase database)
    {
        // Companies collection indexes
        var companiesCollection = database.GetCollection<CompanyMongo>("companies");

        var indexKeys = Builders<CompanyMongo>.IndexKeys;
        var indexOptions = new CreateIndexOptions { Unique = false };

        // Name index
        await companiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyMongo>(
                indexKeys.Text(c => c.Name),
                indexOptions));

        // TaxCode unique index (only for non-deleted documents)
        var taxCodeIndex = Builders<CompanyMongo>.IndexKeys.Ascending(c => c.TaxCode);
        var taxCodeOptions = new CreateIndexOptions
        {
            Unique = true
        };
        // Note: Partial indexes require MongoDB 3.2+ and specific syntax
        // For now, we'll handle uniqueness in application logic
        await companiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyMongo>(taxCodeIndex, taxCodeOptions));

        // DomainId index for cross-references
        await companiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyMongo>(
                indexKeys.Ascending(c => c.DomainId),
                new CreateIndexOptions { Unique = true }));

        // Users collection indexes
        var usersCollection = database.GetCollection<UserMongo>("users");

        // Username index
        var usernameIndex = Builders<UserMongo>.IndexKeys.Ascending(u => u.Username);
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongo>(usernameIndex, new CreateIndexOptions { Unique = true }));

        // Email index
        var emailIndex = Builders<UserMongo>.IndexKeys.Ascending(u => u.Email);
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongo>(emailIndex, new CreateIndexOptions { Unique = true }));

        // DomainId index for cross-references
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongo>(
                Builders<UserMongo>.IndexKeys.Ascending(u => u.DomainId),
                new CreateIndexOptions { Unique = true }));
    }
}