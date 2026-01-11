using FAM.Domain.Users;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Seeders;

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

        User admin = User.Create(
            "admin",
            "admin@gmail.com",
            "Admin@123",
            "System",
            "Administrator",
            "0901234567",
            "+84"
        );
        admin.VerifyEmail();
        admin.UpdatePersonalInfo(
            "System",
            "Administrator",
            "https://gravatar.com/userimage/244508532/70e488bb05c1956db10a6d7673009e76.jpeg?size=1024",
            null,
            null
        );

        List<User> users = new() { admin };

        await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {users.Count} users successfully");
    }
}
