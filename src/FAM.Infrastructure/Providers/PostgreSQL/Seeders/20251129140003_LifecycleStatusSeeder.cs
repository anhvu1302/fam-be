using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial lifecycle statuses for assets
/// </summary>
public class LifecycleStatusSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public LifecycleStatusSeeder(PostgreSqlDbContext dbContext, ILogger<LifecycleStatusSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140003_LifecycleStatusSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing lifecycle statuses...");

        var hasStatuses = await _dbContext.LifecycleStatuses.AnyAsync(s => !s.IsDeleted, cancellationToken);

        if (hasStatuses)
        {
            LogInfo("Lifecycle statuses already exist, skipping seed");
            return;
        }

        LogInfo("Seeding lifecycle statuses...");

        var statuses = new List<LifecycleStatusEf>
        {
            new()
            {
                Code = "NEW",
                Name = "New",
                Description = "Asset is newly acquired and not yet in use",
                Color = "#4CAF50",
                OrderNo = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "ACTIVE",
                Name = "Active",
                Description = "Asset is currently in active use",
                Color = "#2196F3",
                OrderNo = 2,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "MAINTENANCE",
                Name = "Under Maintenance",
                Description = "Asset is currently being maintained or repaired",
                Color = "#FF9800",
                OrderNo = 3,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "RETIRED",
                Name = "Retired",
                Description = "Asset has been retired from service",
                Color = "#9E9E9E",
                OrderNo = 4,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "DISPOSED",
                Name = "Disposed",
                Description = "Asset has been disposed or sold",
                Color = "#F44336",
                OrderNo = 5,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "LOST",
                Name = "Lost",
                Description = "Asset is missing or cannot be located",
                Color = "#E91E63",
                OrderNo = 6,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "DAMAGED",
                Name = "Damaged",
                Description = "Asset is damaged beyond repair",
                Color = "#795548",
                OrderNo = 7,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.LifecycleStatuses.AddRangeAsync(statuses, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created {statuses.Count} lifecycle statuses");
    }
}
