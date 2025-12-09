using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Menu.Commands.CreateMenu;

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, MenuItemResponse>
{
    private readonly IMenuItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateMenuCommandHandler> _logger;
    private const int MaxMenuDepth = 3;

    public CreateMenuCommandHandler(
        IMenuItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateMenuCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MenuItemResponse> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken: cancellationToken))
            throw new DomainException(ErrorCodes.MENU_CODE_EXISTS);

        // Validate parent if provided
        MenuItem? parent = null;
        if (request.ParentId.HasValue)
        {
            parent = await _repository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent == null) throw new DomainException(ErrorCodes.MENU_INVALID_PARENT);

            // Check max depth
            if (parent.Level >= MaxMenuDepth - 1) throw new DomainException(ErrorCodes.MENU_MAX_DEPTH_EXCEEDED);
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

        if (parent != null) menu.SetParent(parent);

        if (!string.IsNullOrEmpty(request.ExternalUrl))
            menu.SetExternalUrl(request.ExternalUrl, request.OpenInNewTab);

        await _repository.AddAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created menu item: {Code}", request.Code);

        return MenuItemResponse.FromDomain(menu, false);
    }
}