using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;

namespace FAM.Domain.Assets;

/// <summary>
/// Bàn giao / thu hồi tài sản
/// </summary>
public class Assignment : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    // Domain fields
    public long AssetId { get; private set; }
    public string AssigneeType { get; private set; } = string.Empty; // user, department, location
    public int AssigneeId { get; private set; }
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; private set; }
    public int? ByUserId { get; private set; }
    public string? Comments { get; private set; }

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
    public User? ByUser { get; set; }

    private Assignment()
    {
    }

    public static Assignment Create(long assetId, string assigneeType, int assigneeId, int? byUserId = null,
        string? comments = null)
    {
        return new Assignment
        {
            AssetId = assetId,
            AssigneeType = assigneeType,
            AssigneeId = assigneeId,
            AssignedAt = DateTime.UtcNow,
            ByUserId = byUserId,
            Comments = comments
        };
    }

    public void Release()
    {
        ReleasedAt = DateTime.UtcNow;
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
