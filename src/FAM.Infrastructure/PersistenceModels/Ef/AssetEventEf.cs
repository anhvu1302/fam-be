using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for AssetEvent
/// </summary>
[Table("asset_events")]
public class AssetEventEf : EntityEf
{
    public long AssetId { get; set; }
    public long? EventTypeId { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public long? ActorId { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
    public string? FromLifecycleCode { get; set; }
    public string? ToLifecycleCode { get; set; }
    public string? Payload { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public AssetEf Asset { get; set; } = null!;
    public AssetEventTypeEf EventType { get; set; } = null!;
    public UserEf? Actor { get; set; }
}