using FAM.Application.Menu.DTOs;

using MediatR;

namespace FAM.Application.Menu.Commands.CreateMenu;

public record CreateMenuCommand(
    string Code,
    string Name,
    string? Description = null,
    string? Icon = null,
    string? Route = null,
    string? ExternalUrl = null,
    long? ParentId = null,
    int SortOrder = 0,
    string? RequiredPermission = null,
    string? RequiredRoles = null,
    bool OpenInNewTab = false) : IRequest<MenuItemResponse>;
