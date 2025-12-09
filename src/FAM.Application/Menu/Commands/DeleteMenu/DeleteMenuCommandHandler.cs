using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Menu.Commands.DeleteMenu;

public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand>
{
    private readonly IMenuItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteMenuCommandHandler> _logger;

    public DeleteMenuCommandHandler(
        IMenuItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteMenuCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (menu == null) throw new NotFoundException(ErrorCodes.MENU_NOT_FOUND, "MenuItem", request.Id);

        // Check if has children
        if (await _repository.HasChildrenAsync(request.Id, cancellationToken))
            throw new DomainException(ErrorCodes.MENU_HAS_CHILDREN);

        _repository.Delete(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted menu item: {Id}", request.Id);
    }
}