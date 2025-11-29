using FAM.Application.Menu.DTOs;
using FAM.Application.Menu.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using FAM.Domain.Common.Entities;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// Implementation of menu item service
/// </summary>
public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MenuItemService> _logger;
    private const int MaxMenuDepth = 3;

    public MenuItemService(
        IMenuItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MenuItemService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<MenuItemFlatResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var menus = await _repository.GetAllAsync(cancellationToken);
        return menus.Select(MenuItemFlatResponse.FromDomain);
    }

    public async Task<MenuItemResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var menu = await _repository.GetByIdAsync(id, cancellationToken);
        return menu != null ? MenuItemResponse.FromDomain(menu, includeChildren: true) : null;
    }

    public async Task<MenuItemResponse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var menu = await _repository.GetByCodeAsync(code, cancellationToken);
        return menu != null ? MenuItemResponse.FromDomain(menu, includeChildren: true) : null;
    }

    public async Task<IEnumerable<MenuItemResponse>> GetMenuTreeAsync(int maxDepth = 3, CancellationToken cancellationToken = default)
    {
        maxDepth = Math.Min(maxDepth, MaxMenuDepth);
        var menus = await _repository.GetMenuTreeAsync(maxDepth, cancellationToken);
        return menus.Select(m => MenuItemResponse.FromDomain(m, includeChildren: true, maxDepth));
    }

    public async Task<IEnumerable<MenuItemResponse>> GetVisibleMenuTreeAsync(
        IEnumerable<string>? userPermissions,
        IEnumerable<string>? userRoles,
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        maxDepth = Math.Min(maxDepth, MaxMenuDepth);
        var menus = await _repository.GetVisibleMenuTreeAsync(userPermissions, userRoles, maxDepth, cancellationToken);
        return menus.Select(m => MenuItemResponse.FromDomain(m, includeChildren: true, maxDepth));
    }

    public async Task<MenuItemResponse> CreateAsync(CreateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        // Check if code already exists
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken: cancellationToken))
        {
            throw new DomainException(ErrorCodes.MENU_CODE_EXISTS);
        }

        // Validate parent if provided
        MenuItem? parent = null;
        if (request.ParentId.HasValue)
        {
            parent = await _repository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent == null)
            {
                throw new DomainException(ErrorCodes.MENU_INVALID_PARENT);
            }

            // Check max depth
            if (parent.Level >= MaxMenuDepth - 1)
            {
                throw new DomainException(ErrorCodes.MENU_MAX_DEPTH_EXCEEDED);
            }
        }

        var menu = MenuItem.Create(
            request.Code,
            request.Name,
            request.Description,
            request.Icon,
            request.Route,
            request.ParentId,
            request.SortOrder,
            request.RequiredPermission,
            request.RequiredRoles);

        if (parent != null)
        {
            menu.SetParent(parent);
        }

        if (!string.IsNullOrEmpty(request.ExternalUrl))
        {
            menu.SetExternalUrl(request.ExternalUrl, request.OpenInNewTab);
        }

        await _repository.AddAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created menu item: {Code}", request.Code);

        return MenuItemResponse.FromDomain(menu, includeChildren: false);
    }

    public async Task<MenuItemResponse> UpdateAsync(long id, UpdateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        var menu = await _repository.GetByIdAsync(id, cancellationToken);
        if (menu == null)
        {
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", id);
        }

        // Validate parent if changed
        if (request.ParentId.HasValue && request.ParentId != menu.ParentId)
        {
            if (request.ParentId.Value == id)
            {
                throw new DomainException(ErrorCodes.MENU_CIRCULAR_REFERENCE);
            }

            var parent = await _repository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent == null)
            {
                throw new DomainException(ErrorCodes.MENU_INVALID_PARENT);
            }

            if (parent.Level >= MaxMenuDepth - 1)
            {
                throw new DomainException(ErrorCodes.MENU_MAX_DEPTH_EXCEEDED);
            }

            menu.SetParent(parent);
        }
        else if (request.ParentId == null && menu.ParentId.HasValue)
        {
            menu.SetParent(null);
        }

        menu.Update(
            request.Name,
            request.Description,
            request.Icon,
            request.Route,
            request.RequiredPermission,
            request.RequiredRoles);

        if (request.SortOrder.HasValue)
        {
            menu.SetSortOrder(request.SortOrder.Value);
        }

        if (request.IsVisible.HasValue)
        {
            if (request.IsVisible.Value) menu.Show();
            else menu.Hide();
        }

        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value) menu.Enable();
            else menu.Disable();
        }

        if (request.ExternalUrl != null || request.OpenInNewTab.HasValue)
        {
            menu.SetExternalUrl(request.ExternalUrl, request.OpenInNewTab ?? menu.OpenInNewTab);
        }

        if (request.CssClass != null)
        {
            menu.SetCssClass(request.CssClass);
        }

        if (request.Badge != null || request.BadgeVariant != null)
        {
            menu.SetBadge(request.Badge, request.BadgeVariant ?? menu.BadgeVariant);
        }

        _repository.Update(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated menu item: {Id}", id);

        return MenuItemResponse.FromDomain(menu, includeChildren: false);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var menu = await _repository.GetByIdAsync(id, cancellationToken);
        if (menu == null)
        {
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", id);
        }

        // Check if has children
        if (await _repository.HasChildrenAsync(id, cancellationToken))
        {
            throw new DomainException(ErrorCodes.MENU_HAS_CHILDREN);
        }

        _repository.Delete(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted menu item: {Id}", id);
    }

    public async Task UpdateSortOrdersAsync(UpdateMenuSortOrdersRequest request, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateSortOrdersAsync(request.SortOrders, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated sort orders for {Count} menu items", request.SortOrders.Count);
    }

    public async Task<MenuItemResponse> MoveAsync(long id, long? newParentId, int sortOrder, CancellationToken cancellationToken = default)
    {
        var menu = await _repository.GetByIdAsync(id, cancellationToken);
        if (menu == null)
        {
            throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", id);
        }

        MenuItem? newParent = null;
        if (newParentId.HasValue)
        {
            if (newParentId.Value == id)
            {
                throw new DomainException(ErrorCodes.MENU_CIRCULAR_REFERENCE);
            }

            newParent = await _repository.GetByIdAsync(newParentId.Value, cancellationToken);
            if (newParent == null)
            {
                throw new DomainException(ErrorCodes.MENU_INVALID_PARENT);
            }

            if (newParent.Level >= MaxMenuDepth - 1)
            {
                throw new DomainException(ErrorCodes.MENU_MAX_DEPTH_EXCEEDED);
            }
        }

        menu.SetParent(newParent);
        menu.SetSortOrder(sortOrder);

        _repository.Update(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Moved menu item: {Id} to parent: {ParentId}", id, newParentId);

        return MenuItemResponse.FromDomain(menu, includeChildren: false);
    }
}
