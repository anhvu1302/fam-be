using FAM.Domain.Common;

namespace FAM.Domain.Assets;

/// <summary>
/// Bàn giao / thu hồi tài sản
/// </summary>
public class Assignment : Entity
{
    public long AssetId { get; private set; }
    public string AssigneeType { get; private set; } = string.Empty; // user, department, location
    public int AssigneeId { get; private set; }
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; private set; }
    public int? ByUserId { get; private set; }
    public string? Comments { get; private set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
    public Users.User? ByUser { get; set; }

    private Assignment() { }

    public static Assignment Create(long assetId, string assigneeType, int assigneeId, int? byUserId = null, string? comments = null)
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
}
