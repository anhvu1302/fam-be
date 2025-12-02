using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL repository for MenuItem
/// </summary>
public class MenuItemRepository : IMenuItemRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;
    private const int MaxMenuDepth = 3;

    public MenuItemRepository(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MenuItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MenuItems
            .Include(m => m.Parent)
            .Include(m => m.Children.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<MenuItem>(entity) : null;
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.MenuItems
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<MenuItem>(e));
    }

    public async Task<IEnumerable<MenuItem>> FindAsync(
        Expression<Func<MenuItem, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task AddAsync(MenuItem entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<MenuItemEf>(entity);
        await _context.MenuItems.AddAsync(efEntity, cancellationToken);
    }

    public void Update(MenuItem entity)
    {
        var efEntity = _context.MenuItems.Local.FirstOrDefault(m => m.Id == entity.Id);
        if (efEntity != null)
        {
            _mapper.Map(entity, efEntity);
        }
        else
        {
            efEntity = _mapper.Map<MenuItemEf>(entity);
            _context.MenuItems.Update(efEntity);
        }
    }

    public void Delete(MenuItem entity)
    {
        var efEntity = _context.MenuItems.Local.FirstOrDefault(m => m.Id == entity.Id);
        if (efEntity != null)
        {
            efEntity.IsDeleted = true;
            efEntity.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.AnyAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<MenuItem?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Code == code.ToLowerInvariant() && !m.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<MenuItem>(entity) : null;
    }

    public async Task<IReadOnlyList<MenuItem>> GetRootMenusAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.MenuItems
            .Where(m => m.ParentId == null && !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<MenuItem>(e)).ToList();
    }

    public async Task<IReadOnlyList<MenuItem>> GetByParentIdAsync(long parentId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.MenuItems
            .Where(m => m.ParentId == parentId && !m.IsDeleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<MenuItem>(e)).ToList();
    }

    public async Task<IReadOnlyList<MenuItem>> GetMenuTreeAsync(int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        maxDepth = Math.Min(maxDepth, MaxMenuDepth);

        // Get all menu items ordered by sort order
        var allMenus = await _context.MenuItems
            .Where(m => !m.IsDeleted && m.Level < maxDepth)
            .OrderBy(m => m.Level)
            .ThenBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

        // Build tree structure in memory
        var menuDict = allMenus.ToDictionary(m => m.Id);
        var rootMenus = new List<MenuItemEf>();

        foreach (var menu in allMenus)
            if (menu.ParentId == null)
            {
                rootMenus.Add(menu);
            }
            else if (menuDict.TryGetValue(menu.ParentId.Value, out var parent))
            {
                parent.Children.Add(menu);
                menu.Parent = parent;
            }

        return rootMenus.Select(e => _mapper.Map<MenuItem>(e)).ToList();
    }

    public async Task<IReadOnlyList<MenuItem>> GetVisibleMenuTreeAsync(
        IEnumerable<string>? userPermissions,
        IEnumerable<string>? userRoles,
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        var tree = await GetMenuTreeAsync(maxDepth, cancellationToken);

        // Filter visible menus based on permissions/roles
        var permissionSet = userPermissions?.ToHashSet() ?? new HashSet<string>();
        var roleSet = userRoles?.ToHashSet() ?? new HashSet<string>();

        return FilterVisibleMenus(tree, permissionSet, roleSet).ToList();
    }

    private IEnumerable<MenuItem> FilterVisibleMenus(
        IEnumerable<MenuItem> menus,
        HashSet<string> permissions,
        HashSet<string> roles)
    {
        foreach (var menu in menus)
            if (menu.CanView(permissions, roles))
                // Note: Children filtering would need to be done at domain level
                // since MenuItem.Children is read-only
                yield return menu;
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems.Where(m => m.Code == code.ToLowerInvariant() && !m.IsDeleted);
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
        foreach (var kvp in sortOrders)
            await _context.MenuItems
                .Where(m => m.Id == kvp.Key && !m.IsDeleted)
                .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.SortOrder, kvp.Value)
                        .SetProperty(m => m.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);
    }
}