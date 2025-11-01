using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Authorization role
/// </summary>
public class Role : BaseEntity
{
    public RoleCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Rank { get; private set; }

    // Navigation properties
    public ICollection<UserNodeRole> UserNodeRoles { get; set; } = new List<UserNodeRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    private Role() { }

    public static Role Create(string code, string name, int rank, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty");

        return new Role
        {
            Code = RoleCode.Create(code),
            Name = name.Trim(),
            Description = description?.Trim(),
            Rank = rank
        };
    }

    public void Update(string name, int rank, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty");

        Name = name.Trim();
        Description = description?.Trim();
        Rank = rank;
    }
}