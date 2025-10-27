using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Mongo;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Seeders;

/// <summary>
/// Seeds asset categories for MongoDB
/// </summary>
public class MongoDbAssetCategorySeeder : BaseDataSeeder
{
    private readonly MongoDbContext _dbContext;

    public MongoDbAssetCategorySeeder(MongoDbContext dbContext, ILogger<MongoDbAssetCategorySeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override int Order => 2;

    public override string Name => "MongoDB Asset Category Seeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed asset categories...");

        var categoriesCollection = _dbContext.GetCollection<AssetCategoryMongo>("asset_categories");

        // Check if categories already exist
        var count = await categoriesCollection.CountDocumentsAsync(
            FilterDefinition<AssetCategoryMongo>.Empty,
            cancellationToken: cancellationToken);

        if (count > 0)
        {
            LogInfo("Asset categories already exist, skipping seed");
            return;
        }

        var categories = new List<AssetCategoryMongo>
        {
            new AssetCategoryMongo
            {
                DomainId = 1,
                Name = "IT Equipment",
                Code = "IT",
                Description = "Information Technology Equipment",
                Level = 1,
                IsActive = true,
                IsDeleted = false,
                IsDepreciable = true,
                IsCapitalized = true,
                RequiresMaintenance = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AssetCategoryMongo
            {
                DomainId = 2,
                Name = "Office Furniture",
                Code = "FURN",
                Description = "Office and Workspace Furniture",
                Level = 1,
                IsActive = true,
                IsDeleted = false,
                IsDepreciable = true,
                IsCapitalized = true,
                RequiresMaintenance = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AssetCategoryMongo
            {
                DomainId = 3,
                Name = "Vehicles",
                Code = "VEH",
                Description = "Company Vehicles",
                Level = 1,
                IsActive = true,
                IsDeleted = false,
                IsDepreciable = true,
                IsCapitalized = true,
                RequiresMaintenance = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await categoriesCollection.InsertManyAsync(categories, cancellationToken: cancellationToken);

        LogInfo($"Seeded {categories.Count} asset categories successfully");
    }
}
