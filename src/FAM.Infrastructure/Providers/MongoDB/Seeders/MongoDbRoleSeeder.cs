using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Mongo;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Seeders;

/// <summary>
/// Seeds initial roles data for MongoDB
/// </summary>
public class MongoDbRoleSeeder : BaseDataSeeder
{
    private readonly MongoDbContext _dbContext;

    public MongoDbRoleSeeder(MongoDbContext dbContext, ILogger<MongoDbRoleSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140002_MongoDbRoleSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed roles...");

        var rolesCollection = _dbContext.GetCollection<RoleMongo>("roles");

        // Check if roles already exist
        var count = await rolesCollection.CountDocumentsAsync(
            FilterDefinition<RoleMongo>.Empty,
            cancellationToken: cancellationToken);

        if (count > 0)
        {
            LogInfo("Roles already exist, skipping seed");
            return;
        }

        var roles = new[]
        {
            new RoleMongo(1)
            {
                Name = "Admin",
                Description =
                    "System Administrator: Full access to all system features including role and permission management",
                Code = "ADMIN",
                Rank = 1
            },
            new RoleMongo(2)
            {
                Name = "FA_MANAGER",
                Description = "FA Manager: phê duyệt tạo FA, phê duyệt thanh lý (write-off). Xem được tất cả",
                Code = "FA_MANAGER",
                Rank = 2
            },
            new RoleMongo(3)
            {
                Name = "FA_WORKER",
                Description =
                    "FA Worker: trực tiếp thao tác tất cả action. Tạo, sửa, report... vào làm và xem được tất cả",
                Code = "FA_WORKER",
                Rank = 3
            },
            new RoleMongo(4)
            {
                Name = "FIN_STAFF",
                Description = "FIN Staff: xem, sửa được thông tin finance thôi. Report FIN",
                Code = "FIN_STAFF",
                Rank = 4
            },
            new RoleMongo(5)
            {
                Name = "PIC",
                Description =
                    "PIC: người quản lý tài sản, sửa tình trạng, location của FA, nhưng không dc cấp phát hay thu hồi",
                Code = "PIC",
                Rank = 5
            },
            new RoleMongo(6)
            {
                Name = "STAFF",
                Description = "Staff: người được cấp tài sản, chỉ dc xem thông tin tài sản dc cấp",
                Code = "STAFF",
                Rank = 6
            }
        };

        await rolesCollection.InsertManyAsync(roles, cancellationToken: cancellationToken);

        LogInfo($"Seeded {roles.Length} roles successfully");
    }
}