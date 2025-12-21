using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial roles data for PostgreSQL
/// </summary>
public class RoleSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public RoleSeeder(PostgreSqlDbContext dbContext, ILogger<RoleSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140002_RoleSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed roles...");

        // Check if roles already exist
        if (await _dbContext.Roles.AnyAsync(cancellationToken))
        {
            LogInfo("Roles already exist, skipping seed");
            return;
        }

        Role[] roles = new[]
        {
            Role.Create(
                "ADMIN",
                "Administrator",
                1,
                "System Administrator: Full access to all system features",
                true
            ),
            Role.Create(
                "STAFF",
                "Staff",
                10,
                "General staff role - parent role for specific staff roles",
                true
            ),
            Role.Create(
                "FA_MANAGER",
                "FA Manager",
                15,
                "Fixed Asset Manager: All FA Worker permissions + management capabilities (approve/disapprove, reports)",
                true
            ),
            Role.Create(
                "FA_WORKER",
                "FA Worker",
                20,
                "Fixed Asset Worker: Search, create, approve/disapprove assets",
                true
            ),
            Role.Create(
                "FIN_STAFF",
                "Finance Staff",
                25,
                "Finance Staff: View all reports and export to Excel",
                true
            ),
            Role.Create(
                "PIC",
                "PIC (Person In Charge)",
                30,
                "Person In Charge: Can only view assets they manage",
                true
            )
        };

        await _dbContext.Roles.AddRangeAsync(roles, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {roles.Length} roles successfully");
    }
}
