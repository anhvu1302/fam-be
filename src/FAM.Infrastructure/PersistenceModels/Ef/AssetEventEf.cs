using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for AssetEvent
/// </summary>
[Table("asset_events")]
public class AssetEventEf : BaseEntityEf
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


    // Navigation properties
    public AssetEf Asset { get; set; } = null!;
    public AssetEventTypeEf EventType { get; set; } = null!;
    public UserEf? Actor { get; set; }
}
