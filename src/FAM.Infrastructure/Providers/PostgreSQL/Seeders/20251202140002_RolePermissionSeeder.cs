using FAM.Domain.Authorization;
using FAM.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds default role-permission assignments based on new role structure
/// Admin, Staff (parent), FA_WORKER, FA_MANAGER, PIC, FIN_STAFF
/// Uses Pragmatic Architecture - works with Domain Permission entity directly
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
        Role? adminRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.Admin, cancellationToken);
        Role? staffRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.Staff, cancellationToken);
        Role? faWorkerRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.FAWorker, cancellationToken);
        Role? faManagerRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.FAManager, cancellationToken);
        Role? picRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.PIC, cancellationToken);
        Role? finStaffRole =
            await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == RoleCodes.FinStaff, cancellationToken);

        if (adminRole == null)
        {
            LogWarning("Admin role not found, skipping role-permission seed");
            return;
        }

        // Get all permissions
        List<Permission> allPermissions = await _dbContext.Permissions.ToListAsync(cancellationToken);
        var rolePermissions = new List<RolePermission>();

        // ==================== ADMIN - Full access to everything ====================
        if (adminRole != null)
        {
            LogInfo("Assigning ALL permissions to Admin role");
            foreach (Permission permission in allPermissions)
                rolePermissions.Add(RolePermission.Create(adminRole.Id, permission.Id));
        }

        // ==================== FA WORKER - Search, create, approve/disapprove assets ====================
        if (faWorkerRole != null)
        {
            LogInfo("Assigning permissions to FA Worker role");
            var workerPermissions = allPermissions.Where(p =>
            {
                var resource = p.Resource;
                var action = p.Action;
                return
                    // Assets: view, search, create, approve, disapprove
                    (resource == Resources.Assets && (action == Actions.View ||
                                                      action == Actions.Search ||
                                                      action == Actions.Create ||
                                                      action == Actions.Approve ||
                                                      action == Actions.Disapprove)) ||
                    // Categories: view
                    (resource == Resources.Categories && action == Actions.View) ||
                    // Locations: view
                    (resource == Resources.Locations && action == Actions.View) ||
                    // Organizations: view
                    (resource == Resources.Organizations && action == Actions.View) ||
                    // Departments: view
                    (resource == Resources.Departments && action == Actions.View) ||
                    // Suppliers: view
                    (resource == Resources.Suppliers && action == Actions.View) ||
                    // Manufacturers: view
                    (resource == Resources.Manufacturers && action == Actions.View);
            }).ToList();

            foreach (Permission permission in workerPermissions)
                rolePermissions.Add(RolePermission.Create(faWorkerRole.Id, permission.Id));
        }

        // ==================== FA MANAGER - All FA Worker permissions + management ====================
        if (faManagerRole != null)
        {
            LogInfo("Assigning permissions to FA Manager role");
            var managerPermissions = allPermissions.Where(p =>
            {
                var resource = p.Resource;
                var action = p.Action;
                return
                    // Assets: all except delete
                    (resource == Resources.Assets && action != Actions.Delete) ||
                    // Categories: view, create, update
                    (resource == Resources.Categories && (action == Actions.View ||
                                                          action == Actions.Create ||
                                                          action == Actions.Update)) ||
                    // Locations: view, create, update
                    (resource == Resources.Locations && (action == Actions.View ||
                                                         action == Actions.Create ||
                                                         action == Actions.Update)) ||
                    // Organizations: view
                    (resource == Resources.Organizations && action == Actions.View) ||
                    // Departments: view, create, update
                    (resource == Resources.Departments && (action == Actions.View ||
                                                           action == Actions.Create ||
                                                           action == Actions.Update)) ||
                    // Suppliers: view, create, update
                    (resource == Resources.Suppliers && (action == Actions.View ||
                                                         action == Actions.Create ||
                                                         action == Actions.Update)) ||
                    // Manufacturers: view, create, update
                    (resource == Resources.Manufacturers && (action == Actions.View ||
                                                             action == Actions.Create ||
                                                             action == Actions.Update)) ||
                    // Reports: view, create, export
                    (resource == Resources.Reports && (action == Actions.View ||
                                                       action == Actions.Create ||
                                                       action == Actions.Export));
            }).ToList();

            foreach (Permission permission in managerPermissions)
                rolePermissions.Add(RolePermission.Create(faManagerRole.Id, permission.Id));
        }

        // ==================== PIC - Only view managed assets ====================
        if (picRole != null)
        {
            LogInfo("Assigning permissions to PIC role");
            var picPermissions = allPermissions.Where(p =>
            {
                var resource = p.Resource;
                var action = p.Action;
                return
                    // Assets: only view owned
                    (resource == Resources.Assets && action == Actions.ViewOwned) ||
                    // Categories: view
                    (resource == Resources.Categories && action == Actions.View) ||
                    // Locations: view
                    (resource == Resources.Locations && action == Actions.View);
            }).ToList();

            foreach (Permission permission in picPermissions)
                rolePermissions.Add(RolePermission.Create(picRole.Id, permission.Id));
        }

        // ==================== FIN STAFF - View reports, export Excel ====================
        if (finStaffRole != null)
        {
            LogInfo("Assigning permissions to Finance Staff role");
            var finPermissions = allPermissions.Where(p =>
            {
                var resource = p.Resource;
                var action = p.Action;
                return
                    // Finance: view
                    (resource == Resources.Finance && action == Actions.View) ||
                    // Reports: view all, create, export, export Excel
                    (resource == Resources.Reports && (action == Actions.View ||
                                                       action == Actions.ViewAll ||
                                                       action == Actions.Create ||
                                                       action == Actions.Export ||
                                                       action == Actions.ExportExcel)) ||
                    // Assets: view (for reporting)
                    (resource == Resources.Assets && action == Actions.View);
            }).ToList();

            foreach (Permission permission in finPermissions)
                rolePermissions.Add(RolePermission.Create(finStaffRole.Id, permission.Id));
        }

        // Convert domain entities to EF entities
        await _dbContext.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {rolePermissions.Count} role-permission assignments successfully");
    }
}
