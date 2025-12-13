using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for AssetCondition
/// </summary>
[Table("asset_conditions")]
public class AssetConditionEf : BaseEntityEf
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}
