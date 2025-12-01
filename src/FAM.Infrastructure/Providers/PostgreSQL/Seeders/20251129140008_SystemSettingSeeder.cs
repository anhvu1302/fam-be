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
            CreateSetting("app.general.siteDescription", "Site Description", "Fixed Asset Management System",
                group: "general", sortOrder: 1, dataType: 1),
            CreateSetting("app.general.timezone", "Default Timezone", "Asia/Ho_Chi_Minh", group: "general",
                sortOrder: 2),
            CreateSetting("app.general.dateFormat", "Date Format", "dd/MM/yyyy", group: "general", sortOrder: 3),
            CreateSetting("app.general.currency", "Default Currency", "VND", group: "general", sortOrder: 4),
            CreateSetting("app.general.language", "Default Language", "vi", group: "general", sortOrder: 5),

            // Branding settings
            CreateSetting("app.branding.logo", "Logo URL", "/images/logo.png", group: "branding", sortOrder: 0,
                dataType: 12),
            CreateSetting("app.branding.favicon", "Favicon URL", "/images/favicon.ico", group: "branding", sortOrder: 1,
                dataType: 12),
            CreateSetting("app.branding.primaryColor", "Primary Color", "#1976d2", group: "branding", sortOrder: 2,
                dataType: 10),
            CreateSetting("app.branding.secondaryColor", "Secondary Color", "#424242", group: "branding", sortOrder: 3,
                dataType: 10),

            // Footer settings
            CreateSetting("app.footer.copyright", "Copyright Text", "© 2024 FAM System. All rights reserved.",
                group: "footer", sortOrder: 0),
            CreateSetting("app.footer.company", "Company Name", "FAM Company", group: "footer", sortOrder: 1),
            CreateSetting("app.footer.address", "Company Address", "", group: "footer", sortOrder: 2, dataType: 1),
            CreateSetting("app.footer.phone", "Contact Phone", "", group: "footer", sortOrder: 3),
            CreateSetting("app.footer.email", "Contact Email", "", group: "footer", sortOrder: 4, dataType: 9),
            CreateSetting("app.footer.showSocialLinks", "Show Social Links", "false", group: "footer", sortOrder: 5,
                dataType: 4),

            // Email settings
            CreateSetting("app.email.smtpHost", "SMTP Host", "", group: "email", sortOrder: 0, isSensitive: false),
            CreateSetting("app.email.smtpPort", "SMTP Port", "587", group: "email", sortOrder: 1, dataType: 2),
            CreateSetting("app.email.smtpUser", "SMTP Username", "", group: "email", sortOrder: 2),
            CreateSetting("app.email.smtpPassword", "SMTP Password", "", group: "email", sortOrder: 3,
                isSensitive: true, dataType: 16),
            CreateSetting("app.email.fromAddress", "From Email Address", "noreply@fam.local", group: "email",
                sortOrder: 4, dataType: 9),
            CreateSetting("app.email.fromName", "From Name", "FAM System", group: "email", sortOrder: 5),

            // Security settings
            CreateSetting("app.security.sessionTimeout", "Session Timeout (minutes)", "60", group: "security",
                sortOrder: 0, dataType: 2, isEditable: true),
            CreateSetting("app.security.maxLoginAttempts", "Max Login Attempts", "5", group: "security", sortOrder: 1,
                dataType: 2),
            CreateSetting("app.security.lockoutDuration", "Lockout Duration (minutes)", "15", group: "security",
                sortOrder: 2, dataType: 2),
            CreateSetting("app.security.passwordMinLength", "Password Min Length", "8", group: "security", sortOrder: 3,
                dataType: 2),
            CreateSetting("app.security.require2FA", "Require 2FA", "false", group: "security", sortOrder: 4,
                dataType: 4),

            // Asset settings
            CreateSetting("app.asset.codePrefix", "Asset Code Prefix", "AST", group: "asset", sortOrder: 0),
            CreateSetting("app.asset.codeAutoGenerate", "Auto Generate Asset Code", "true", group: "asset",
                sortOrder: 1, dataType: 4),
            CreateSetting("app.asset.depreciationMethod", "Default Depreciation Method", "straight-line",
                group: "asset", sortOrder: 2, dataType: 14,
                options:
                "[{\"value\":\"straight-line\",\"label\":\"Straight Line\"},{\"value\":\"declining-balance\",\"label\":\"Declining Balance\"},{\"value\":\"sum-of-years\",\"label\":\"Sum of Years Digits\"}]"),
            CreateSetting("app.asset.defaultLifespan", "Default Asset Lifespan (years)", "5", group: "asset",
                sortOrder: 3, dataType: 2)
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