using FAM.Domain.Common;

namespace FAM.Domain.Assets;

/// <summary>
/// Sự kiện tài sản (audit log)
/// </summary>
public class AssetEvent : Entity
{
    public long AssetId { get; private set; }
    public string EventCode { get; private set; } = string.Empty;
    public int? ActorId { get; private set; }
    public DateTime At { get; private set; } = DateTime.UtcNow;
    public string? FromLifecycleCode { get; private set; }
    public string? ToLifecycleCode { get; private set; }
    public string? Payload { get; private set; } // JSONB stored as string
    public string? Note { get; private set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
    public Statuses.AssetEventType EventType { get; set; } = null!;
    public Users.User? Actor { get; set; }

    private AssetEvent() { }

    public static AssetEvent Create(
        long assetId,
        string eventCode,
        int? actorId = null,
        string? fromLifecycleCode = null,
        string? toLifecycleCode = null,
        string? payload = null,
        string? note = null)
    {
        return new AssetEvent
        {
            AssetId = assetId,
            EventCode = eventCode,
            ActorId = actorId,
            At = DateTime.UtcNow,
            FromLifecycleCode = fromLifecycleCode,
            ToLifecycleCode = toLifecycleCode,
            Payload = payload,
            Note = note
        };
    }
}
