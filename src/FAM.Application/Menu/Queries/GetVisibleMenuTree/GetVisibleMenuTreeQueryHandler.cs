using FAM.Application.Menu.DTOs;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Menu.Queries.GetVisibleMenuTree;

public class GetVisibleMenuTreeQueryHandler : IRequestHandler<GetVisibleMenuTreeQuery, IEnumerable<MenuItemResponse>>
{
    private readonly IMenuItemRepository _repository;
    private const int MaxMenuDepth = 3;

    public GetVisibleMenuTreeQueryHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuItemResponse>> Handle(GetVisibleMenuTreeQuery request,
        CancellationToken cancellationToken)
    {
        int maxDepth = Math.Min(request.MaxDepth, MaxMenuDepth);
        IReadOnlyList<MenuItem> menus = await _repository.GetVisibleMenuTreeAsync(
            request.UserPermissions,
            request.UserRoles,
            maxDepth,
            cancellationToken);
        return menus.Select(m => MenuItemResponse.FromDomain(m, true, maxDepth));
    }
}
