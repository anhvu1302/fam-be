using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Statuses;
using FAM.Domain.Users;

namespace FAM.Domain.Assets;

/// <summary>
/// Sự kiện tài sản (audit log)
/// </summary>
public class AssetEvent : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    // Domain fields
    public long AssetId { get; private set; }
    public string EventCode { get; private set; } = string.Empty;
    public int? ActorId { get; private set; }
    public DateTime At { get; private set; } = DateTime.UtcNow;
    public string? FromLifecycleCode { get; private set; }
    public string? ToLifecycleCode { get; private set; }
    public string? Payload { get; private set; } // JSONB stored as string
    public string? Note { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
    public User? DeletedBy { get; set; }
    public Asset Asset { get; set; } = null!;
    public AssetEventType EventType { get; set; } = null!;
    public User? Actor { get; set; }

    private AssetEvent()
    {
    }

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

    public void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
