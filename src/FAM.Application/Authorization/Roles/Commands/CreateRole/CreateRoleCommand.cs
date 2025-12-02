using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.CreateRole;

/// <summary>
/// Command to create a new role
/// </summary>
public sealed record CreateRoleCommand : IRequest<long>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Rank { get; init; }
    public bool IsSystemRole { get; init; }
}