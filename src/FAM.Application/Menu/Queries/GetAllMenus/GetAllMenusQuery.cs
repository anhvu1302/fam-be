using FAM.Application.Menu.DTOs;
using MediatR;

namespace FAM.Application.Menu.Queries.GetAllMenus;

public record GetAllMenusQuery : IRequest<IEnumerable<MenuItemFlatResponse>>;