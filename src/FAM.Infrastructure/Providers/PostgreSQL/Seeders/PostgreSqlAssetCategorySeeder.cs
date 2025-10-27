using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds asset categories for PostgreSQL
/// </summary>
public class PostgreSqlAssetCategorySeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public PostgreSqlAssetCategorySeeder(PostgreSqlDbContext dbContext, ILogger<PostgreSqlAssetCategorySeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override int Order => 3;

    public override string Name => "PostgreSQL Asset Category Seeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed asset categories...");

        if (await _dbContext.AssetCategories.AnyAsync(cancellationToken))
        {
            LogInfo("Asset categories already exist, skipping seed");
            return;
        }

        var categories = new List<AssetCategoryEf>
        {
            new AssetCategoryEf
            {
                Name = "IT Equipment",
                Code = "IT",
                Description = "Information Technology Equipment",
                Level = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AssetCategoryEf
            {
                Name = "Office Furniture",
                Code = "FURN",
                Description = "Office and Workspace Furniture",
                Level = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AssetCategoryEf
            {
                Name = "Vehicles",
                Code = "VEH",
                Description = "Company Vehicles",
                Level = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.AssetCategories.AddRangeAsync(categories, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {categories.Count} asset categories successfully");
    }
}
