using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for Permission
/// </summary>
[Table("permissions")]
public class PermissionEf : BaseEntityEf
{
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<RolePermissionEf> RolePermissions { get; set; } = new List<RolePermissionEf>();
}

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

    // Navigation properties
    public ICollection<UserNodeRoleEf> UserNodeRoles { get; set; } = new List<UserNodeRoleEf>();
    public ICollection<RolePermissionEf> RolePermissions { get; set; } = new List<RolePermissionEf>();
}

/// <summary>
/// EF persistence model for Resource
/// </summary>
[Table("resources")]
public class ResourceEf : BaseEntityEf
{
    public string Type { get; set; } = string.Empty;
    public long NodeId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public OrgNodeEf? Node { get; set; }
}

/// <summary>
/// EF persistence model for RolePermission (junction table)
/// </summary>
[Table("role_permissions")]
public class RolePermissionEf : BaseEntityEf
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }

    // Navigation properties
    public RoleEf? Role { get; set; }
    public PermissionEf? Permission { get; set; }
}

/// <summary>
/// EF persistence model for UserNodeRole (junction table)
/// </summary>
[Table("user_node_roles")]
public class UserNodeRoleEf : BaseEntityEf
{
    public long UserId { get; set; }
    public long NodeId { get; set; }
    public long RoleId { get; set; }

    // Navigation properties
    public UserEf? User { get; set; }
    public OrgNodeEf? Node { get; set; }
    public RoleEf? Role { get; set; }
}