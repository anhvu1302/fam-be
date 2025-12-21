using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL repository for MenuItem
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class MenuItemRepository : IMenuItemRepository
{
    private readonly PostgreSqlDbContext _context;
    private const int MaxMenuDepth = 3;

    public MenuItemRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Include(m => m.Parent)
            .Include(m => m.Children.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> FindAsync(
        Expression<Func<MenuItem, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await _context.MenuItems.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MenuItem entity, CancellationToken cancellationToken = default)
    {
        await _context.MenuItems.AddAsync(entity, cancellationToken);
    }

    public void Update(MenuItem entity)
    {
        var trackedEntry = _context.ChangeTracker.Entries<MenuItem>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _context.MenuItems.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(MenuItem entity)
    {
        var trackedEntity = _context.MenuItems.Local.FirstOrDefault(m => m.Id == entity.Id);
        if (trackedEntity != null)
        {
            trackedEntity.IsDeleted = true;
            trackedEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.AnyAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<MenuItem?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Code == code.ToLowerInvariant() && !m.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetRootMenusAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.ParentId == null && !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetByParentIdAsync(long parentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Where(m => m.ParentId == parentId && !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetMenuTreeAsync(int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        maxDepth = Math.Min(maxDepth, MaxMenuDepth);

        // Get all menu items ordered by sort order
        List<MenuItem> allMenus = await _context.MenuItems
            .Where(m => !m.IsDeleted && m.Level < maxDepth)
            .OrderBy(m => m.Level)
            .ThenBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

        // Build tree structure in memory
        var menuDict = allMenus.ToDictionary(m => m.Id);
        var rootMenus = new List<MenuItem>();

        foreach (MenuItem menu in allMenus)
            if (menu.ParentId == null)
            {
                rootMenus.Add(menu);
            }
            else if (menuDict.TryGetValue(menu.ParentId.Value, out MenuItem? parent))
            {
                parent.Children.Add(menu);
            }

        return rootMenus;
    }

    public async Task<IReadOnlyList<MenuItem>> GetVisibleMenuTreeAsync(
        IEnumerable<string>? userPermissions,
        IEnumerable<string>? userRoles,
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MenuItem> tree = await GetMenuTreeAsync(maxDepth, cancellationToken);

        // Filter visible menus based on permissions/roles
        HashSet<string> permissionSet = userPermissions?.ToHashSet() ?? new HashSet<string>();
        HashSet<string> roleSet = userRoles?.ToHashSet() ?? new HashSet<string>();

        return FilterVisibleMenus(tree, permissionSet, roleSet).ToList();
    }

    private IEnumerable<MenuItem> FilterVisibleMenus(
        IEnumerable<MenuItem> menus,
        HashSet<string> permissions,
        HashSet<string> roles)
    {
        foreach (MenuItem menu in menus)
            if (menu.CanView(permissions, roles))
                // Note: Children filtering would need to be done at domain level
                // since MenuItem.Children is read-only
                yield return menu;
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MenuItem> query = _context.MenuItems.Where(m => m.Code == code.ToLowerInvariant() && !m.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(long menuId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.AnyAsync(m => m.ParentId == menuId && !m.IsDeleted, cancellationToken);
    }

    public async Task UpdateSortOrdersAsync(Dictionary<long, int> sortOrders,
        CancellationToken cancellationToken = default)
    {
        foreach (KeyValuePair<long, int> kvp in sortOrders)
            await _context.MenuItems
                .Where(m => m.Id == kvp.Key && !m.IsDeleted)
                .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.SortOrder, kvp.Value)
                        .SetProperty(m => m.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);
    }
}
