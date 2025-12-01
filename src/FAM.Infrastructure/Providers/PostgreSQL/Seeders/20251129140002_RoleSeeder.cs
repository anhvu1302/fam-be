using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;
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

        var roles = new[]
        {
            new RoleEf
            {
                Name = "Admin",
                Description =
                    "System Administrator: Full access to all system features including role and permission management",
                Code = "ADMIN",
                Rank = 1
            },
            new RoleEf
            {
                Name = "FA_MANAGER",
                Description = "FA Manager: phê duyệt tạo FA, phê duyệt thanh lý (write-off). Xem được tất cả",
                Code = "FA_MANAGER",
                Rank = 2
            },
            new RoleEf
            {
                Name = "FA_WORKER",
                Description =
                    "FA Worker: trực tiếp thao tác tất cả action. Tạo, sửa, report... vào làm và xem được tất cả",
                Code = "FA_WORKER",
                Rank = 3
            },
            new RoleEf
            {
                Name = "FIN_STAFF",
                Description = "FIN Staff: xem, sửa được thông tin finance thôi. Report FIN",
                Code = "FIN_STAFF",
                Rank = 4
            },
            new RoleEf
            {
                Name = "PIC",
                Description =
                    "PIC: người quản lý tài sản, sửa tình trạng, location của FA, nhưng không dc cấp phát hay thu hồi",
                Code = "PIC",
                Rank = 5
            },
            new RoleEf
            {
                Name = "STAFF",
                Description = "Staff: người được cấp tài sản, chỉ dc xem thông tin tài sản dc cấp",
                Code = "STAFF",
                Rank = 6
            }
        };

        await _dbContext.Roles.AddRangeAsync(roles, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {roles.Length} roles successfully");
    }
}