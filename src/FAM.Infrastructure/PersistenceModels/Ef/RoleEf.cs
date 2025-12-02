using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for Role
/// </summary>
[Table("roles")]
public class RoleEf : BaseEntityEf
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Rank { get; set; }
    public bool IsSystemRole { get; set; }

    // Navigation properties
    public ICollection<UserNodeRoleEf> UserNodeRoles { get; set; } = new List<UserNodeRoleEf>();
    public ICollection<RolePermissionEf> RolePermissions { get; set; } = new List<RolePermissionEf>();
}