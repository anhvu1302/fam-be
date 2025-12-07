using FAM.Application.Menu.DTOs;
using MediatR;

namespace FAM.Application.Menu.Queries.GetMenuById;

public record GetMenuByIdQuery(long Id) : IRequest<MenuItemResponse?>;
