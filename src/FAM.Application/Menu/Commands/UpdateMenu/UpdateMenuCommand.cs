using FAM.Application.Menu.DTOs;
using MediatR;

namespace FAM.Application.Menu.Commands.UpdateMenu;

public record UpdateMenuCommand(
    long Id,
    string Name,
    string? Description = null,
    string? Icon = null,
    string? Route = null,
    string? ExternalUrl = null,
    long? ParentId = null,
    int? SortOrder = null,
    string? RequiredPermission = null,
    string? RequiredRoles = null,
    bool? OpenInNewTab = null,
    bool? IsVisible = null,
    bool? IsEnabled = null,
    string? CssClass = null,
    string? Badge = null,
    string? BadgeVariant = null) : IRequest<MenuItemResponse>;
