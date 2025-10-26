using FAM.Infrastructure.Common.Options;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB;

/// <summary>
/// MongoDB context wrapper
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbOptions options)
    {
        var client = new MongoClient(options.ConnectionString);
        _database = client.GetDatabase(options.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase Database => _database;
}