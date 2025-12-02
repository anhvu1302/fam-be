using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for AssetCondition
/// </summary>
[Table("asset_conditions")]
public class AssetConditionEf : BaseEntityEf
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    // Navigation properties
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}