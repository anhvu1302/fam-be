using FAM.Application.Menu.DTOs;
using MediatR;

namespace FAM.Application.Menu.Commands.MoveMenu;

public record MoveMenuCommand(long Id, long? NewParentId, int SortOrder) : IRequest<MenuItemResponse>;
