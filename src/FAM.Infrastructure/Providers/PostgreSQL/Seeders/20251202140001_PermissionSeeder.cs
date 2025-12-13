using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds all system permissions from Permissions constants
/// </summary>
public class PermissionSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public PermissionSeeder(PostgreSqlDbContext dbContext, ILogger<PermissionSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251202140001_PermissionSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed permissions...");

        // Check if permissions already exist
        if (await _dbContext.Permissions.AnyAsync(cancellationToken))
        {
            LogInfo("Permissions already exist, skipping seed");
            return;
        }

        // Get all predefined permissions from Domain
        IReadOnlyList<(string Resource, string Action, string Description)> allPermissions = Permissions.All;

        var permissionEntities = new List<PermissionEf>();

        foreach (var (resource, action, description) in allPermissions)
            permissionEntities.Add(new PermissionEf
            {
                Resource = resource,
                Action = action
            });

        await _dbContext.Permissions.AddRangeAsync(permissionEntities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {permissionEntities.Count} permissions successfully");
    }
}
