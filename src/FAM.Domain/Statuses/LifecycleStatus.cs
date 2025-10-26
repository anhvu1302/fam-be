using FAM.Domain.Common;

namespace FAM.Domain.Statuses;

/// <summary>
/// Trạng thái vòng đời của tài sản (Nháp, Chờ duyệt, Đã duyệt, Đang hoạt động, v.v.)
/// </summary>
public class LifecycleStatus : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Color { get; private set; }
    public int? OrderNo { get; private set; }

    // Navigation properties
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();

    private LifecycleStatus() { }

    public static LifecycleStatus Create(string code, string name, string? description = null, string? color = null, int? orderNo = null)
    {
        return new LifecycleStatus
        {
            Code = code,
            Name = name,
            Description = description,
            Color = color,
            OrderNo = orderNo
        };
    }
}
