using FAM.Domain.Common.Entities;
using FAM.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial menu items for the application
/// </summary>
public class MenuSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public MenuSeeder(PostgreSqlDbContext dbContext, ILogger<MenuSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140007_MenuSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing menu items...");

        // Check if menus already exist
        var hasMenus = await _dbContext.MenuItems.AnyAsync(m => !m.IsDeleted, cancellationToken);

        if (hasMenus)
        {
            LogInfo("Menu items already exist, skipping seed");
            return;
        }

        LogInfo("Seeding initial menu items...");

        var menus = new List<MenuItem>();

        // Dashboard
        MenuItem dashboard = CreateMenu("dashboard", "Dashboard", "dashboard", "/dashboard", sortOrder: 0);
        menus.Add(dashboard);

        // Asset Management (Parent)
        MenuItem assets = CreateMenu("assets", "Asset Management", "inventory_2", null, sortOrder: 1);
        menus.Add(assets);

        // Reports (Parent)
        MenuItem reports = CreateMenu("reports", "Reports", "assessment", null, sortOrder: 2);
        menus.Add(reports);

        // Settings (Parent)
        MenuItem settings = CreateMenu("settings", "Settings", "settings", null, sortOrder: 3,
            requiredRoles: "Admin,SuperAdmin");
        menus.Add(settings);

        await _dbContext.MenuItems.AddRangeAsync(menus, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Now add children with proper parent IDs
        var childMenus = new List<MenuItem>();

        // Asset Management children
        childMenus.Add(CreateMenu("assets.list", "All Assets", "list", "/assets", assets.Id, 0, 1));
        childMenus.Add(CreateMenu("assets.categories", "Categories", "category", "/assets/categories", assets.Id, 1,
            1));
        childMenus.Add(CreateMenu("assets.types", "Asset Types", "type_specimen", "/assets/types", assets.Id, 2, 1));
        childMenus.Add(CreateMenu("assets.locations", "Locations", "location_on", "/assets/locations", assets.Id, 3,
            1));

        // Reports children
        childMenus.Add(CreateMenu("reports.overview", "Overview", "dashboard", "/reports/overview", reports.Id, 0, 1));
        childMenus.Add(CreateMenu("reports.depreciation", "Depreciation", "trending_down", "/reports/depreciation",
            reports.Id, 1, 1));
        childMenus.Add(CreateMenu("reports.audit", "Audit Trail", "history", "/reports/audit", reports.Id, 2, 1));

        // Settings children
        childMenus.Add(CreateMenu("settings.general", "General", "tune", "/settings/general", settings.Id, 0, 1,
            "Admin,SuperAdmin"));
        childMenus.Add(CreateMenu("settings.users", "Users", "people", "/settings/users", settings.Id, 1, 1,
            "Admin,SuperAdmin"));
        childMenus.Add(CreateMenu("settings.roles", "Roles", "admin_panel_settings", "/settings/roles", settings.Id, 2,
            1, "Admin,SuperAdmin"));
        childMenus.Add(CreateMenu("settings.menus", "Menu Management", "menu", "/settings/menus", settings.Id, 3, 1,
            "SuperAdmin"));
        childMenus.Add(CreateMenu("settings.system", "System Settings", "settings_applications", "/settings/system",
            settings.Id, 4, 1, "SuperAdmin"));

        await _dbContext.MenuItems.AddRangeAsync(childMenus, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Created {menus.Count + childMenus.Count} menu items");
    }

    private static MenuItem CreateMenu(
        string code,
        string name,
        string? icon = null,
        string? route = null,
        long? parentId = null,
        int sortOrder = 0,
        int level = 0,
        string? requiredRoles = null)
    {
        return MenuItem.Create(
            code: code,
            name: name,
            icon: icon,
            route: route,
            parentId: parentId,
            sortOrder: sortOrder,
            requiredRoles: requiredRoles);
    }
}
