namespace FAM.Domain.Authorization;

/// <summary>
/// Permission constants for authorization system
/// Defines all available permissions in the system
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Gets all defined permissions in the system
    /// </summary>
    public static IReadOnlyList<(string Resource, string Action, string Description)> All => new[]
    {
        // User Management (Admin only)
        (Resources.Users, Actions.View, "View users"),
        (Resources.Users, Actions.Create, "Create new users"),
        (Resources.Users, Actions.Update, "Update user information"),
        (Resources.Users, Actions.Delete, "Delete users"),
        (Resources.Users, Actions.Activate, "Activate/Deactivate users"),

        // Role Management (Admin only)
        (Resources.Roles, Actions.View, "View roles"),
        (Resources.Roles, Actions.Create, "Create new roles"),
        (Resources.Roles, Actions.Update, "Update role information"),
        (Resources.Roles, Actions.Delete, "Delete roles"),
        (Resources.Roles, Actions.Assign, "Assign roles to users"),

        // Permission Management (Admin only)
        (Resources.Permissions, Actions.View, "View permissions"),
        (Resources.Permissions, Actions.Assign, "Assign permissions to roles"),

        // Asset Management (FA Workers & FA Managers)
        (Resources.Assets, Actions.View, "View assets"),
        (Resources.Assets, Actions.Search, "Search assets"),
        (Resources.Assets, Actions.Create, "Create new assets"),
        (Resources.Assets, Actions.Update, "Update asset information"),
        (Resources.Assets, Actions.Delete, "Delete assets"),
        (Resources.Assets, Actions.Approve, "Approve asset requests"),
        (Resources.Assets, Actions.Disapprove, "Disapprove asset requests"),
        (Resources.Assets, Actions.Assign, "Assign assets to users"),
        (Resources.Assets, Actions.Transfer, "Transfer assets between users"),
        (Resources.Assets, Actions.Audit, "Audit assets"),
        (Resources.Assets, Actions.ViewOwned, "View only owned/managed assets (PIC)"),

        // Category Management
        (Resources.Categories, Actions.View, "View asset categories"),
        (Resources.Categories, Actions.Create, "Create categories"),
        (Resources.Categories, Actions.Update, "Update categories"),
        (Resources.Categories, Actions.Delete, "Delete categories"),

        // Location Management
        (Resources.Locations, Actions.View, "View locations"),
        (Resources.Locations, Actions.Create, "Create locations"),
        (Resources.Locations, Actions.Update, "Update locations"),
        (Resources.Locations, Actions.Delete, "Delete locations"),

        // Organization Management
        (Resources.Organizations, Actions.View, "View organization structure"),
        (Resources.Organizations, Actions.Create, "Create organization nodes"),
        (Resources.Organizations, Actions.Update, "Update organization nodes"),
        (Resources.Organizations, Actions.Delete, "Delete organization nodes"),

        // Department Management
        (Resources.Departments, Actions.View, "View departments"),
        (Resources.Departments, Actions.Create, "Create departments"),
        (Resources.Departments, Actions.Update, "Update departments"),
        (Resources.Departments, Actions.Delete, "Delete departments"),

        // Supplier Management
        (Resources.Suppliers, Actions.View, "View suppliers"),
        (Resources.Suppliers, Actions.Create, "Create suppliers"),
        (Resources.Suppliers, Actions.Update, "Update suppliers"),
        (Resources.Suppliers, Actions.Delete, "Delete suppliers"),

        // Manufacturer Management
        (Resources.Manufacturers, Actions.View, "View manufacturers"),
        (Resources.Manufacturers, Actions.Create, "Create manufacturers"),
        (Resources.Manufacturers, Actions.Update, "Update manufacturers"),
        (Resources.Manufacturers, Actions.Delete, "Delete manufacturers"),

        // Finance Management (Fin Staff)
        (Resources.Finance, Actions.View, "View financial information"),
        (Resources.Finance, Actions.Create, "Create financial entries"),
        (Resources.Finance, Actions.Update, "Update financial entries"),
        (Resources.Finance, Actions.Delete, "Delete financial entries"),
        (Resources.Finance, Actions.Approve, "Approve financial transactions"),

        // Report Management (Fin Staff - can filter all, export Excel)
        (Resources.Reports, Actions.View, "View reports"),
        (Resources.Reports, Actions.ViewAll, "View all reports (no filter restrictions)"),
        (Resources.Reports, Actions.Create, "Generate reports"),
        (Resources.Reports, Actions.Export, "Export reports"),
        (Resources.Reports, Actions.ExportExcel, "Export reports to Excel"),

        // System Settings (Admin only)
        (Resources.Settings, Actions.View, "View system settings"),
        (Resources.Settings, Actions.Update, "Update system settings"),

        // Audit Logs (Admin only)
        (Resources.AuditLogs, Actions.View, "View audit logs")
    };

    /// <summary>
    /// Get permission key in format: resource:action
    /// </summary>
    public static string GetPermissionKey(string resource, string action)
    {
        return $"{resource}:{action}";
    }

    /// <summary>
    /// Check if a permission exists in the system
    /// </summary>
    public static bool IsValidPermission(string resource, string action)
    {
        return All.Any(p => p.Resource == resource && p.Action == action);
    }
}

/// <summary>
/// Resource types for authorization
/// </summary>
public static class Resources
{
    public const string Users = "users";
    public const string Roles = "roles";
    public const string Permissions = "permissions";
    public const string Assets = "assets";
    public const string Categories = "categories";
    public const string Locations = "locations";
    public const string Organizations = "organizations";
    public const string Departments = "departments";
    public const string Suppliers = "suppliers";
    public const string Manufacturers = "manufacturers";
    public const string Finance = "finance";
    public const string Reports = "reports";
    public const string Settings = "settings";
    public const string AuditLogs = "audit_logs";

    public static IReadOnlyList<string> All => new[]
    {
        Users, Roles, Permissions, Assets, Categories, Locations,
        Organizations, Departments, Suppliers, Manufacturers,
        Finance, Reports, Settings, AuditLogs
    };

    public static bool IsValid(string resource)
    {
        return All.Contains(resource);
    }
}

/// <summary>
/// Action types for authorization
/// </summary>
public static class Actions
{
    public const string View = "view";
    public const string ViewAll = "view_all";
    public const string ViewOwned = "view_owned";
    public const string Search = "search";
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Assign = "assign";
    public const string Transfer = "transfer";
    public const string Approve = "approve";
    public const string Disapprove = "disapprove";
    public const string Activate = "activate";
    public const string Audit = "audit";
    public const string Export = "export";
    public const string ExportExcel = "export_excel";

    public static IReadOnlyList<string> All => new[]
    {
        View, ViewAll, ViewOwned, Search, Create, Update, Delete,
        Assign, Transfer, Approve, Disapprove, Activate, Audit,
        Export, ExportExcel
    };

    public static bool IsValid(string action)
    {
        return All.Contains(action);
    }
}

/// <summary>
/// Predefined role codes
/// </summary>
public static class RoleCodes
{
    // Top level roles
    public const string Admin = "ADMIN";
    public const string Staff = "STAFF";

    // Staff sub-roles
    public const string FAWorker = "FA_WORKER"; // Can search, create, approve, disapprove assets
    public const string FAManager = "FA_MANAGER"; // All FA Worker permissions + management
    public const string PIC = "PIC"; // Person In Charge - only view managed assets
    public const string FinStaff = "FIN_STAFF"; // Finance Staff - view reports, export Excel

    public static IReadOnlyList<string> All => new[]
    {
        Admin, Staff,
        FAWorker, FAManager,
        PIC, FinStaff
    };

    public static bool IsValid(string code)
    {
        return All.Contains(code);
    }

    /// <summary>
    /// Check if role code is a Staff sub-role
    /// </summary>
    public static bool IsStaffRole(string code)
    {
        return code is FAWorker or FAManager or PIC or FinStaff;
    }

    /// <summary>
    /// Check if role code is a FA Worker role
    /// </summary>
    public static bool IsFAWorkerRole(string code)
    {
        return code is FAWorker;
    }

    /// <summary>
    /// Check if role code is a FA Manager role
    /// </summary>
    public static bool IsFAManagerRole(string code)
    {
        return code is FAManager;
    }

    /// <summary>
    /// Check if role code is finance-related
    /// </summary>
    public static bool IsFinanceRole(string code)
    {
        return code is FinStaff;
    }
}