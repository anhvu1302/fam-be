using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Menu.Commands.UpdateMenu;

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, MenuItemResponse>
{
    private readonly IMenuItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMenuCommandHandler> _logger;
    private const int MaxMenuDepth = 3;

    public UpdateMenuCommandHandler(
        IMenuItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateMenuCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MenuItemResponse> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (menu == null) throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", request.Id);

        // Validate parent if changed
        if (request.ParentId.HasValue && request.ParentId != menu.ParentId)
        {
            if (request.ParentId.Value == request.Id) throw new DomainException(ErrorCodes.MENU_CIRCULAR_REFERENCE);

            var parent = await _repository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent == null) throw new DomainException(ErrorCodes.MENU_INVALID_PARENT);

            if (parent.Level >= MaxMenuDepth - 1) throw new DomainException(ErrorCodes.MENU_MAX_DEPTH_EXCEEDED);

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

        if (request.SortOrder.HasValue) menu.SetSortOrder(request.SortOrder.Value);

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
            menu.SetExternalUrl(request.ExternalUrl, request.OpenInNewTab ?? menu.OpenInNewTab);

        if (request.CssClass != null) menu.SetCssClass(request.CssClass);

        if (request.Badge != null || request.BadgeVariant != null)
            menu.SetBadge(request.Badge, request.BadgeVariant ?? menu.BadgeVariant);

        _repository.Update(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated menu item: {Id}", request.Id);

        return MenuItemResponse.FromDomain(menu, false);
    }
}