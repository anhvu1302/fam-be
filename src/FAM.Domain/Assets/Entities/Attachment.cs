using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;

namespace FAM.Domain.Assets;

/// <summary>
/// File đính kèm cho tài sản
/// </summary>
public class Attachment : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter
{
    // Domain fields
    public long? AssetId { get; private set; }
    public string? FileName { get; private set; }
    public string? FileUrl { get; private set; }
    public int? UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

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
    public Asset? Asset { get; set; }
    public User? Uploader { get; set; }

    private Attachment()
    {
    }

    public static Attachment Create(long? assetId, string fileName, string fileUrl, int? uploadedBy = null)
    {
        return new Attachment
        {
            AssetId = assetId,
            FileName = fileName,
            FileUrl = fileUrl,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
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