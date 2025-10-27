using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Attachment
/// </summary>
[Table("asset_attachments")]
public class AttachmentEf : EntityEf
{
    public long? AssetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AssetEf? Asset { get; set; }
    public UserEf? Uploader { get; set; }
}