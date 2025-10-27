using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FAM.Domain.Statuses;
using FAM.Infrastructure.Common;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for LifecycleStatus
/// </summary>
[Table("lifecycle_statuses")]
public class LifecycleStatusEf : BaseEntityEf
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public int? OrderNo { get; set; }

    // Navigation properties
    public ICollection<AssetEf> Assets { get; set; } = new List<AssetEf>();
}