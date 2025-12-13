using FAM.Application.Menu.DTOs;

using MediatR;

namespace FAM.Application.Menu.Queries.GetMenuTree;

public record GetMenuTreeQuery(int MaxDepth = 3) : IRequest<IEnumerable<MenuItemResponse>>;
