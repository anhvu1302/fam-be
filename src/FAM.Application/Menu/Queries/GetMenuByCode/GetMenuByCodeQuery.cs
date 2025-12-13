using FAM.Application.Menu.DTOs;

using MediatR;

namespace FAM.Application.Menu.Queries.GetMenuByCode;

public record GetMenuByCodeQuery(string Code) : IRequest<MenuItemResponse?>;
