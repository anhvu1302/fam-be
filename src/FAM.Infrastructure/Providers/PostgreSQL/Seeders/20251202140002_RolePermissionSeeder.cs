using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds default role-permission assignments based on new role structure
/// Admin, Staff (parent), FA_WORKER, FA_MANAGER, PIC, FIN_STAFF
/// </summary>
public class RolePermissionSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public RolePermissionSeeder(PostgreSqlDbContext dbContext, ILogger<RolePermissionSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251202140002_RolePermissionSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed role-permission assignments...");

        // Check if role-permissions already exist
        if (await _dbContext.RolePermissions.AnyAsync(cancellationToken))
        {
            LogInfo("Role-permissions already exist, skipping seed");
            return;
        }

        // Get roles
        RoleEf? adminRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.Admin, cancellationToken);
        RoleEf? staffRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.Staff, cancellationToken);
        RoleEf? faWorkerRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.FAWorker, cancellationToken);
        RoleEf? faManagerRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.FAManager, cancellationToken);
        RoleEf? picRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.PIC, cancellationToken);
        RoleEf? finStaffRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.FinStaff, cancellationToken);

        if (adminRole == null)
        {
            LogWarning("Admin role not found, skipping role-permission seed");
            return;
        }

        // Get all permissions
        List<PermissionEf> allPermissions = await _dbContext.Permissions.ToListAsync(cancellationToken);
        var rolePermissions = new List<RolePermissionEf>();

        // ==================== ADMIN - Full access to everything ====================
        if (adminRole != null)
        {
            LogInfo("Assigning ALL permissions to Admin role");
            foreach (PermissionEf permission in allPermissions)
                rolePermissions.Add(new RolePermissionEf
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
        }

        // ==================== FA WORKER - Search, create, approve/disapprove assets ====================
        if (faWorkerRole != null)
        {
            LogInfo("Assigning permissions to FA Worker role");
            var workerPermissions = allPermissions.Where(p =>
                // Assets: view, search, create, approve, disapprove
                (p.Resource == Resources.Assets && (p.Action == Actions.View ||
                                                    p.Action == Actions.Search ||
                                                    p.Action == Actions.Create ||
                                                    p.Action == Actions.Approve ||
                                                    p.Action == Actions.Disapprove)) ||
                // Categories: view
                (p.Resource == Resources.Categories && p.Action == Actions.View) ||
                // Locations: view
                (p.Resource == Resources.Locations && p.Action == Actions.View) ||
                // Organizations: view
                (p.Resource == Resources.Organizations && p.Action == Actions.View) ||
                // Departments: view
                (p.Resource == Resources.Departments && p.Action == Actions.View) ||
                // Suppliers: view
                (p.Resource == Resources.Suppliers && p.Action == Actions.View) ||
                // Manufacturers: view
                (p.Resource == Resources.Manufacturers && p.Action == Actions.View)
            ).ToList();

            foreach (PermissionEf permission in workerPermissions)
                rolePermissions.Add(new RolePermissionEf
                {
                    RoleId = faWorkerRole.Id,
                    PermissionId = permission.Id
                });
        }

        // ==================== FA MANAGER - All FA Worker permissions + management ====================
        if (faManagerRole != null)
        {
            LogInfo("Assigning permissions to FA Manager role");
            var managerPermissions = allPermissions.Where(p =>
                // Assets: all except delete
                (p.Resource == Resources.Assets && p.Action != Actions.Delete) ||
                // Categories: view, create, update
                (p.Resource == Resources.Categories && (p.Action == Actions.View ||
                                                        p.Action == Actions.Create ||
                                                        p.Action == Actions.Update)) ||
                // Locations: view, create, update
                (p.Resource == Resources.Locations && (p.Action == Actions.View ||
                                                       p.Action == Actions.Create ||
                                                       p.Action == Actions.Update)) ||
                // Organizations: view
                (p.Resource == Resources.Organizations && p.Action == Actions.View) ||
                // Departments: view, create, update
                (p.Resource == Resources.Departments && (p.Action == Actions.View ||
                                                         p.Action == Actions.Create ||
                                                         p.Action == Actions.Update)) ||
                // Suppliers: view, create, update
                (p.Resource == Resources.Suppliers && (p.Action == Actions.View ||
                                                       p.Action == Actions.Create ||
                                                       p.Action == Actions.Update)) ||
                // Manufacturers: view, create, update
                (p.Resource == Resources.Manufacturers && (p.Action == Actions.View ||
                                                           p.Action == Actions.Create ||
                                                           p.Action == Actions.Update)) ||
                // Reports: view, create, export
                (p.Resource == Resources.Reports && (p.Action == Actions.View ||
                                                     p.Action == Actions.Create ||
                                                     p.Action == Actions.Export))
            ).ToList();

            foreach (PermissionEf permission in managerPermissions)
                rolePermissions.Add(new RolePermissionEf
                {
                    RoleId = faManagerRole.Id,
                    PermissionId = permission.Id
                });
        }

        // ==================== PIC - Only view managed assets ====================
        if (picRole != null)
        {
            LogInfo("Assigning permissions to PIC role");
            var picPermissions = allPermissions.Where(p =>
                // Assets: only view owned
                (p.Resource == Resources.Assets && p.Action == Actions.ViewOwned) ||
                // Categories: view
                (p.Resource == Resources.Categories && p.Action == Actions.View) ||
                // Locations: view
                (p.Resource == Resources.Locations && p.Action == Actions.View)
            ).ToList();

            foreach (PermissionEf permission in picPermissions)
                rolePermissions.Add(new RolePermissionEf
                {
                    RoleId = picRole.Id,
                    PermissionId = permission.Id
                });
        }

        // ==================== FIN STAFF - View reports, export Excel ====================
        if (finStaffRole != null)
        {
            LogInfo("Assigning permissions to Finance Staff role");
            var finPermissions = allPermissions.Where(p =>
                // Finance: view
                (p.Resource == Resources.Finance && p.Action == Actions.View) ||
                // Reports: view all, create, export, export Excel
                (p.Resource == Resources.Reports && (p.Action == Actions.View ||
                                                     p.Action == Actions.ViewAll ||
                                                     p.Action == Actions.Create ||
                                                     p.Action == Actions.Export ||
                                                     p.Action == Actions.ExportExcel)) ||
                // Assets: view (for reporting)
                (p.Resource == Resources.Assets && p.Action == Actions.View)
            ).ToList();

            foreach (PermissionEf permission in finPermissions)
                rolePermissions.Add(new RolePermissionEf
                {
                    RoleId = finStaffRole.Id,
                    PermissionId = permission.Id
                });
        }

        await _dbContext.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {rolePermissions.Count} role-permission assignments successfully");
    }
}
