using FAM.Domain.Common;

namespace FAM.Domain.Assets;

/// <summary>
/// File đính kèm cho tài sản
/// </summary>
public class Attachment : Entity
{
    public long? AssetId { get; private set; }
    public string? FileName { get; private set; }
    public string? FileUrl { get; private set; }
    public int? UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    // Navigation properties
    public Asset? Asset { get; set; }
    public Users.User? Uploader { get; set; }

    private Attachment() { }

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
}
