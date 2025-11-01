using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial users data for PostgreSQL
/// </summary>
public class PostgreSqlAdminUserSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public PostgreSqlAdminUserSeeder(PostgreSqlDbContext dbContext, ILogger<PostgreSqlAdminUserSeeder> logger) 
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override int Order => 1;

    public override string Name => "PostgreSQL User Seeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed users...");

        // Check if users already exist
        if (await _dbContext.Users.AnyAsync(cancellationToken))
        {
            LogInfo("Users already exist, skipping seed");
            return;
        }

        var users = new List<UserEf>
        {
            new UserEf
            {
                Username = "admin",
                Email = "admin@fam.local",
                FullName = "System Administrator",
                PasswordHash = "RlI1JkKTEVI6+RhcU/dLzeKshHhDwe3NpWd6Z3BIFtY=", // Hash for Admin@123
                PasswordSalt = "6eb1ccfd64a94810bd3398067dff13f5", // Salt for Admin@123
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
        };

        await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {users.Count} users successfully");
    }
}
