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
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<RolePermissionEf> RolePermissions { get; set; } = new List<RolePermissionEf>();
}