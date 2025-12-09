using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Menu.Queries.GetMenuById;

public class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, MenuItemResponse?>
{
    private readonly IMenuItemRepository _repository;

    public GetMenuByIdQueryHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<MenuItemResponse?> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
    {
        var menu = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return menu != null ? MenuItemResponse.FromDomain(menu, true) : null;
    }
}