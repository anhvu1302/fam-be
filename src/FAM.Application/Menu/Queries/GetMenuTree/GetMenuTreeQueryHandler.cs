using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Menu.Queries.GetMenuTree;

public class GetMenuTreeQueryHandler : IRequestHandler<GetMenuTreeQuery, IEnumerable<MenuItemResponse>>
{
    private readonly IMenuItemRepository _repository;
    private const int MaxMenuDepth = 3;

    public GetMenuTreeQueryHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuItemResponse>> Handle(GetMenuTreeQuery request,
        CancellationToken cancellationToken)
    {
        var maxDepth = Math.Min(request.MaxDepth, MaxMenuDepth);
        var menus = await _repository.GetMenuTreeAsync(maxDepth, cancellationToken);
        return menus.Select(m => MenuItemResponse.FromDomain(m, true, maxDepth));
    }
}