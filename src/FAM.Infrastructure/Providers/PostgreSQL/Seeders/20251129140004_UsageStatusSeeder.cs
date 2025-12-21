using FAM.Domain.Statuses;
using FAM.Infrastructure.Common.Seeding;

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

        var statuses = new List<UsageStatus>
        {
            UsageStatus.Create("AVAILABLE", "Available", "Asset is available for assignment", "#4CAF50", 1),
            UsageStatus.Create("IN_USE", "In Use", "Asset is currently assigned to a user or department", "#2196F3", 2),
            UsageStatus.Create("RESERVED", "Reserved", "Asset is reserved for future assignment", "#9C27B0", 3),
            UsageStatus.Create("IN_TRANSIT", "In Transit", "Asset is being transferred between locations", "#FF9800", 4),
            UsageStatus.Create("IN_REPAIR", "In Repair", "Asset is currently being repaired", "#FFC107", 5),
            UsageStatus.Create("IN_STORAGE", "In Storage", "Asset is stored and not currently in use", "#607D8B", 6),
            UsageStatus.Create("PENDING_DISPOSAL", "Pending Disposal", "Asset is waiting to be disposed", "#F44336", 7),
            UsageStatus.Create("ON_LOAN", "On Loan", "Asset is temporarily loaned to external party", "#00BCD4", 8)
        };

        await _dbContext.UsageStatuses.AddRangeAsync(statuses, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created {statuses.Count} usage statuses");
    }
}
