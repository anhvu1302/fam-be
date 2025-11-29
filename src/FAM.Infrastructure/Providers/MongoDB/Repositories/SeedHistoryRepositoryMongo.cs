using FAM.Infrastructure.Common.Seeding;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Repositories;

public class SeedHistoryRepositoryMongo : ISeedHistoryRepository
{
    private readonly MongoDbContext _dbContext;
    private const string CollectionName = "__seed_history";

    public SeedHistoryRepositoryMongo(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasBeenExecutedAsync(string seederName, CancellationToken cancellationToken = default)
    {
        var collection = _dbContext.GetCollection<SeedHistoryMongo>(CollectionName);

        var filter = Builders<SeedHistoryMongo>.Filter.And(
            Builders<SeedHistoryMongo>.Filter.Eq(h => h.SeederName, seederName),
            Builders<SeedHistoryMongo>.Filter.Eq(h => h.Success, true)
        );

        var count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task RecordExecutionAsync(SeedHistory history, CancellationToken cancellationToken = default)
    {
        var collection = _dbContext.GetCollection<SeedHistoryMongo>(CollectionName);

        var mongoHistory = new SeedHistoryMongo
        {
            SeederName = history.SeederName,
            ExecutedAt = history.ExecutedAt,
            ExecutedBy = history.ExecutedBy,
            Success = history.Success,
            ErrorMessage = history.ErrorMessage,
            DurationMs = history.Duration.TotalMilliseconds
        };

        await collection.InsertOneAsync(mongoHistory, cancellationToken: cancellationToken);
    }

    public async Task<List<SeedHistory>> GetAllHistoryAsync(CancellationToken cancellationToken = default)
    {
        var collection = _dbContext.GetCollection<SeedHistoryMongo>(CollectionName);

        var sort = Builders<SeedHistoryMongo>.Sort.Descending(h => h.ExecutedAt);
        var mongoHistories = await collection.Find(FilterDefinition<SeedHistoryMongo>.Empty)
            .Sort(sort)
            .ToListAsync(cancellationToken);

        return mongoHistories.Select(h => new SeedHistory
        {
            Id = long.Parse(h.Id),
            SeederName = h.SeederName,
            ExecutedAt = h.ExecutedAt,
            ExecutedBy = h.ExecutedBy,
            Success = h.Success,
            ErrorMessage = h.ErrorMessage,
            Duration = TimeSpan.FromMilliseconds(h.DurationMs)
        }).ToList();
    }
}

public class SeedHistoryMongo
{
    public string Id { get; set; } = global::MongoDB.Bson.ObjectId.GenerateNewId().ToString();
    public string SeederName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public string ExecutedBy { get; set; } = "System";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double DurationMs { get; set; }
}