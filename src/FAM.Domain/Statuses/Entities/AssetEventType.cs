using FAM.Domain.Common;

namespace FAM.Domain.Statuses;

/// <summary>
/// Loại sự kiện tài sản (Tạo mới, Gửi duyệt, Duyệt, Bàn giao, Thu hồi, v.v.)
/// </summary>
public class AssetEventType : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Color { get; private set; }
    public int? OrderNo { get; private set; }

    // Navigation properties
    public ICollection<Assets.AssetEvent> AssetEvents { get; set; } = new List<Assets.AssetEvent>();

    private AssetEventType()
    {
    }

    public static AssetEventType Create(string code, string name, string? description = null, string? color = null,
        int? orderNo = null)
    {
        return new AssetEventType
        {
            Code = code,
            Name = name,
            Description = description,
            Color = color,
            OrderNo = orderNo
        };
    }
}