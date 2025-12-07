using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Menu.Commands.UpdateMenuSortOrders;

public class UpdateMenuSortOrdersCommandHandler : IRequestHandler<UpdateMenuSortOrdersCommand>
{
    private readonly IMenuItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMenuSortOrdersCommandHandler> _logger;

    public UpdateMenuSortOrdersCommandHandler(
        IMenuItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateMenuSortOrdersCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdateMenuSortOrdersCommand request, CancellationToken cancellationToken)
    {
        await _repository.UpdateSortOrdersAsync(request.SortOrders, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated sort orders for {Count} menu items", request.SortOrders.Count);
    }
}
