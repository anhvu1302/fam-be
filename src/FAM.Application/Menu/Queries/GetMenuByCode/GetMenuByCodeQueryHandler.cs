using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Menu.Queries.GetMenuByCode;

public class GetMenuByCodeQueryHandler : IRequestHandler<GetMenuByCodeQuery, MenuItemResponse?>
{
    private readonly IMenuItemRepository _repository;

    public GetMenuByCodeQueryHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<MenuItemResponse?> Handle(GetMenuByCodeQuery request, CancellationToken cancellationToken)
    {
        var menu = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        return menu != null ? MenuItemResponse.FromDomain(menu, true) : null;
    }
}
