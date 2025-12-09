using FAM.Application.Menu.DTOs;
using MediatR;

namespace FAM.Application.Menu.Queries.GetVisibleMenuTree;

public record GetVisibleMenuTreeQuery(
    IEnumerable<string>? UserPermissions,
    IEnumerable<string>? UserRoles,
    int MaxDepth = 3) : IRequest<IEnumerable<MenuItemResponse>>;