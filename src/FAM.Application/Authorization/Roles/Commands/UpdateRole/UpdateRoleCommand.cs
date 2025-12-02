using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Command to update an existing role
/// </summary>
public sealed record UpdateRoleCommand : IRequest<bool>
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Rank { get; init; }
}