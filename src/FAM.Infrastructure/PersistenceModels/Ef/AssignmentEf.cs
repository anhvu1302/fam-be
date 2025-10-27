using System.ComponentModel.DataAnnotations.Schema;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for Assignment
/// </summary>
[Table("asset_assignments")]
public class AssignmentEf : EntityEf
{
    public long AssetId { get; set; }
    public string AssigneeType { get; set; } = string.Empty;
    public long AssigneeId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public long? ByUserId { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public AssetEf Asset { get; set; } = null!;
    public UserEf? ByUser { get; set; }
}