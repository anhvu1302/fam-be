using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial usage statuses for assets
/// </summary>
public class UsageStatusSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public UsageStatusSeeder(PostgreSqlDbContext dbContext, ILogger<UsageStatusSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140004_UsageStatusSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing usage statuses...");

        var hasStatuses = await _dbContext.UsageStatuses.AnyAsync(s => !s.IsDeleted, cancellationToken);

        if (hasStatuses)
        {
            LogInfo("Usage statuses already exist, skipping seed");
            return;
        }

        LogInfo("Seeding usage statuses...");

        var statuses = new List<UsageStatusEf>
        {
            new()
            {
                Code = "AVAILABLE",
                Name = "Available",
                Description = "Asset is available for assignment",
                Color = "#4CAF50",
                OrderNo = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "IN_USE",
                Name = "In Use",
                Description = "Asset is currently assigned to a user or department",
                Color = "#2196F3",
                OrderNo = 2,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "RESERVED",
                Name = "Reserved",
                Description = "Asset is reserved for future assignment",
                Color = "#9C27B0",
                OrderNo = 3,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "IN_TRANSIT",
                Name = "In Transit",
                Description = "Asset is being transferred between locations",
                Color = "#FF9800",
                OrderNo = 4,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "IN_REPAIR",
                Name = "In Repair",
                Description = "Asset is currently being repaired",
                Color = "#FFC107",
                OrderNo = 5,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "IN_STORAGE",
                Name = "In Storage",
                Description = "Asset is stored and not currently in use",
                Color = "#607D8B",
                OrderNo = 6,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "PENDING_DISPOSAL",
                Name = "Pending Disposal",
                Description = "Asset is waiting to be disposed",
                Color = "#F44336",
                OrderNo = 7,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Code = "ON_LOAN",
                Name = "On Loan",
                Description = "Asset is temporarily loaned to external party",
                Color = "#00BCD4",
                OrderNo = 8,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.UsageStatuses.AddRangeAsync(statuses, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created {statuses.Count} usage statuses");
    }
}