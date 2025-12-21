using FAM.Domain.Statuses;
using FAM.Infrastructure.Common.Seeding;

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

        var statuses = new List<LifecycleStatus>
        {
            LifecycleStatus.Create("NEW", "New", "Asset is newly acquired and not yet in use", "#4CAF50", 1),
            LifecycleStatus.Create("ACTIVE", "Active", "Asset is currently in active use", "#2196F3", 2),
            LifecycleStatus.Create("MAINTENANCE", "Under Maintenance", "Asset is currently being maintained or repaired", "#FF9800", 3),
            LifecycleStatus.Create("RETIRED", "Retired", "Asset has been retired from service", "#9E9E9E", 4),
            LifecycleStatus.Create("DISPOSED", "Disposed", "Asset has been disposed or sold", "#F44336", 5),
            LifecycleStatus.Create("LOST", "Lost", "Asset is missing or cannot be located", "#E91E63", 6),
            LifecycleStatus.Create("DAMAGED", "Damaged", "Asset is damaged beyond repair", "#795548", 7)
        };

        await _dbContext.LifecycleStatuses.AddRangeAsync(statuses, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created {statuses.Count} lifecycle statuses");
    }
}
