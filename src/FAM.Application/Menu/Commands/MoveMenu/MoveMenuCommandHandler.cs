using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using FAM.Domain.Common.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Menu.Commands.MoveMenu;

public class MoveMenuCommandHandler : IRequestHandler<MoveMenuCommand, MenuItemResponse>
{
    private readonly IMenuItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MoveMenuCommandHandler> _logger;
    private const int MaxMenuDepth = 3;

    public MoveMenuCommandHandler(
        IMenuItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MoveMenuCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MenuItemResponse> Handle(MoveMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (menu == null) throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", request.Id);

        MenuItem? newParent = null;
        if (request.NewParentId.HasValue)
        {
            if (request.NewParentId.Value == request.Id) throw new DomainException(ErrorCodes.MENU_CIRCULAR_REFERENCE);

            newParent = await _repository.GetByIdAsync(request.NewParentId.Value, cancellationToken);
            if (newParent == null) throw new DomainException(ErrorCodes.MENU_INVALID_PARENT);

            if (newParent.Level >= MaxMenuDepth - 1) throw new DomainException(ErrorCodes.MENU_MAX_DEPTH_EXCEEDED);
        }

        menu.SetParent(newParent);
        menu.SetSortOrder(request.SortOrder);

        _repository.Update(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Moved menu item: {Id} to parent: {ParentId}", request.Id, request.NewParentId);

        return MenuItemResponse.FromDomain(menu, false);
    }
}
