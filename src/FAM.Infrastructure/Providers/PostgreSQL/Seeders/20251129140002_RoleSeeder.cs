using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;

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

        RoleEf[] roles = new[]
        {
            new RoleEf
            {
                Name = "Administrator",
                Description = "System Administrator: Full access to all system features",
                Code = "ADMIN",
                Rank = 1,
                IsSystemRole = true
            },
            new RoleEf
            {
                Name = "Staff",
                Description = "General staff role - parent role for specific staff roles",
                Code = "STAFF",
                Rank = 10,
                IsSystemRole = true
            },
            new RoleEf
            {
                Name = "FA Manager",
                Description =
                    "Fixed Asset Manager: All FA Worker permissions + management capabilities (approve/disapprove, reports)",
                Code = "FA_MANAGER",
                Rank = 15,
                IsSystemRole = true
            },
            new RoleEf
            {
                Name = "FA Worker",
                Description = "Fixed Asset Worker: Search, create, approve/disapprove assets",
                Code = "FA_WORKER",
                Rank = 20,
                IsSystemRole = true
            },
            new RoleEf
            {
                Name = "Finance Staff",
                Description = "Finance Staff: View all reports and export to Excel",
                Code = "FIN_STAFF",
                Rank = 25,
                IsSystemRole = true
            },
            new RoleEf
            {
                Name = "PIC (Person In Charge)",
                Description = "Person In Charge: Can only view assets they manage",
                Code = "PIC",
                Rank = 30,
                IsSystemRole = true
            }
        };

        await _dbContext.Roles.AddRangeAsync(roles, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {roles.Length} roles successfully");
    }
}
