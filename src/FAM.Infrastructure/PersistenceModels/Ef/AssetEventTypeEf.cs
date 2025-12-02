using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for AssetEventType
/// </summary>
[Table("asset_event_types")]
public class AssetEventTypeEf : BaseEntityEf
{
    [Required] [MaxLength(20)] public string Code { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    [MaxLength(20)] public string? Color { get; set; }

    public int? OrderNo { get; set; }

    // Navigation properties
    public ICollection<AssetEventEf> AssetEvents { get; set; } = new List<AssetEventEf>();
}