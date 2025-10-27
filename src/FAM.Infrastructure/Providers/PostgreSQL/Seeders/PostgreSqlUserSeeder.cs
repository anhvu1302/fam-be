using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial users data for PostgreSQL
/// </summary>
public class PostgreSqlUserSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public PostgreSqlUserSeeder(PostgreSqlDbContext dbContext, ILogger<PostgreSqlUserSeeder> logger) 
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
                PasswordHash = "", // TODO: Set proper password hash
                PasswordSalt = "", // TODO: Set proper password salt
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserEf
            {
                Username = "manager",
                Email = "manager@fam.local",
                FullName = "Asset Manager",
                PasswordHash = "", // TODO: Set proper password hash
                PasswordSalt = "", // TODO: Set proper password salt
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserEf
            {
                Username = "user",
                Email = "user@fam.local",
                FullName = "Regular User",
                PasswordHash = "", // TODO: Set proper password hash
                PasswordSalt = "", // TODO: Set proper password salt
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {users.Count} users successfully");
    }
}
