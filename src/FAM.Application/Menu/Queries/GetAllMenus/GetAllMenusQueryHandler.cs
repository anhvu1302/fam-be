using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Menu.Queries.GetAllMenus;

public class GetAllMenusQueryHandler : IRequestHandler<GetAllMenusQuery, IEnumerable<MenuItemFlatResponse>>
{
    private readonly IMenuItemRepository _repository;

    public GetAllMenusQueryHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuItemFlatResponse>> Handle(GetAllMenusQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<MenuItem> menus = await _repository.GetAllAsync(cancellationToken);
        return menus.Select(MenuItemFlatResponse.FromDomain);
    }
}
