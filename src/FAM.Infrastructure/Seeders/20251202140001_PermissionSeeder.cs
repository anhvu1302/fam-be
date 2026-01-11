using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Seeders;

/// <summary>
/// Seeds all system permissions from Permissions constants
/// Uses Pragmatic Architecture - directly works with Domain entities
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

        List<Permission> permissionEntities = new();

        foreach ((string resource, string action, string description) in allPermissions)
        {
            permissionEntities.Add(Permission.Create(resource, action, description));
        }

        await _dbContext.Permissions.AddRangeAsync(permissionEntities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {permissionEntities.Count} permissions successfully");
    }
}
