using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial users data for PostgreSQL
/// </summary>
public class AdminUserSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public AdminUserSeeder(PostgreSqlDbContext dbContext, ILogger<AdminUserSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140001_AdminUserSeeder";

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
            new()
            {
                Username = "admin",
                Email = "admin@gmail.com",
                FirstName = "System",
                LastName = "Administrator",
                FullName = "System Administrator",
                Avatar = "https://gravatar.com/userimage/244508532/70e488bb05c1956db10a6d7673009e76.jpeg?size=1024",
                PhoneNumber = "0123456789",
                PhoneCountryCode = "+84",
                PasswordHash = "RlI1JkKTEVI6+RhcU/dLzeKshHhDwe3NpWd6Z3BIFtY=", // Hash for Admin@123
                PasswordSalt = "6eb1ccfd64a94810bd3398067dff13f5", // Salt for Admin@123
                IsEmailVerified = true,
                IsPhoneVerified = false,
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