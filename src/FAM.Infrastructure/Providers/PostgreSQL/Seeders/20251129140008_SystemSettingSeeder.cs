using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial system settings for the application
/// </summary>
public class SystemSettingSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public SystemSettingSeeder(PostgreSqlDbContext dbContext, ILogger<SystemSettingSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140008_SystemSettingSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing system settings...");

        // Check if settings already exist
        var hasSettings = await _dbContext.SystemSettings.AnyAsync(s => !s.IsDeleted, cancellationToken);

        if (hasSettings)
        {
            LogInfo("System settings already exist, skipping seed");
            return;
        }

        LogInfo("Seeding initial system settings...");

        var settings = new List<SystemSettingEf>
        {
            // General settings
            CreateSetting("app.general.siteName", "Site Name", "FAM - Fixed Asset Management", group: "general",
                sortOrder: 0, isRequired: true),
            CreateSetting("app.general.siteDescription", "Site Description",
                "Hệ thống quản lý tài sản cố định | Version 1.0.0",
                group: "general", sortOrder: 1, dataType: 1),

            // Branding settings
            CreateSetting("app.branding.logo", "Logo URL", "/fam-public/images/logo.png", group: "branding",
                sortOrder: 0,
                dataType: 12),
            CreateSetting("app.branding.favicon", "Favicon URL", "/fam-public/images/favicon.ico", group: "branding",
                sortOrder: 1,
                dataType: 12),

            // Footer settings
            CreateSetting("app.footer.copyright", "Copyright Text",
                "© 2025 Fixed Asset Management System. All rights reserved.",
                group: "footer", sortOrder: 0),
            CreateSetting("app.footer.company", "Company Name", "FAM Company", group: "footer", sortOrder: 1),
            CreateSetting("app.footer.address", "Company Address", "", group: "footer", sortOrder: 2, dataType: 1),
            CreateSetting("app.footer.phone", "Contact Phone", "", group: "footer", sortOrder: 3),
            CreateSetting("app.footer.email", "Contact Email", "", group: "footer", sortOrder: 4, dataType: 9),

            // Feature highlights - Feature 1
            CreateSetting("app.features.feature1Title", "Feature 1 Title", "Quản lý tập trung",
                group: "features", sortOrder: 0),
            CreateSetting("app.features.feature1Description", "Feature 1 Description",
                "Theo dõi tất cả tài sản cố định ở một nơi",
                group: "features", sortOrder: 1, dataType: 1),
            CreateSetting("app.features.feature1Icon", "Feature 1 Icon (HTML)",
                "<div class=\"flex h-10 w-10 items-center justify-center rounded-lg bg-white/20\"><svg class=\"h-5 w-5 text-white\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z\"></path></svg></div>",
                group: "features", sortOrder: 2, dataType: 1),

            // Feature highlights - Feature 2
            CreateSetting("app.features.feature2Title", "Feature 2 Title", "Báo cáo thông minh",
                group: "features", sortOrder: 3),
            CreateSetting("app.features.feature2Description", "Feature 2 Description",
                "Tính khấu hao và báo cáo tự động",
                group: "features", sortOrder: 4, dataType: 1),
            CreateSetting("app.features.feature2Icon", "Feature 2 Icon (HTML)",
                "<div class=\"flex h-10 w-10 items-center justify-center rounded-lg bg-white/20\"><svg class=\"h-5 w-5 text-white\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z\"></path></svg></div>",
                group: "features", sortOrder: 5, dataType: 1),

            // Feature highlights - Feature 3
            CreateSetting("app.features.feature3Title", "Feature 3 Title", "Bảo mật cao",
                group: "features", sortOrder: 6),
            CreateSetting("app.features.feature3Description", "Feature 3 Description",
                "Mã hóa dữ liệu và phân quyền chi tiết",
                group: "features", sortOrder: 7, dataType: 1),
            CreateSetting("app.features.feature3Icon", "Feature 3 Icon (HTML)",
                "<div class=\"flex h-10 w-10 items-center justify-center rounded-lg bg-white/20\"><svg class=\"h-5 w-5 text-white\" fill=\"none\" stroke=\"currentColor\" viewBox=\"0 0 24 24\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"2\" d=\"M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z\"></path></svg></div>",
                group: "features", sortOrder: 8, dataType: 1)
        };

        await _dbContext.SystemSettings.AddRangeAsync(settings, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created {settings.Count} system settings");
    }

    private static SystemSettingEf CreateSetting(
        string key,
        string displayName,
        string? value = null,
        string? defaultValue = null,
        int dataType = 0,
        string group = "general",
        string? description = null,
        int sortOrder = 0,
        bool isRequired = false,
        bool isSensitive = false,
        bool isVisible = true,
        bool isEditable = true,
        string? validationRules = null,
        string? options = null)
    {
        return new SystemSettingEf
        {
            Key = key,
            DisplayName = displayName,
            Value = value,
            DefaultValue = defaultValue ?? value,
            DataType = dataType,
            Group = group,
            Description = description,
            SortOrder = sortOrder,
            IsRequired = isRequired,
            IsSensitive = isSensitive,
            IsVisible = isVisible,
            IsEditable = isEditable,
            ValidationRules = validationRules,
            Options = options,
            CreatedAt = DateTime.UtcNow
        };
    }
}
