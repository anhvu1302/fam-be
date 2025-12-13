using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for UserNodeRole (junction table)
/// Composite key: (UserId, NodeId, RoleId)
/// </summary>
[Table("user_node_roles")]
public class UserNodeRoleEf
{
    public long UserId { get; set; }
    public long NodeId { get; set; }
    public long RoleId { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public long? AssignedById { get; set; }

    // Navigation properties
    public UserEf? User { get; set; }
    public OrgNodeEf? Node { get; set; }
    public RoleEf? Role { get; set; }
}
