using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for RolePermission (junction table)
/// Composite key: (RoleId, PermissionId)
/// </summary>
[Table("role_permissions")]
public class RolePermissionEf
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public long? GrantedById { get; set; }

    // Navigation properties
    public RoleEf? Role { get; set; }
    public PermissionEf? Permission { get; set; }
}