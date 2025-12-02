using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

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