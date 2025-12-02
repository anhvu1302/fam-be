using FAM.Domain.Assets;
using FAM.Domain.Common;

namespace FAM.Domain.Statuses;

/// <summary>
/// Trạng thái sử dụng của tài sản (Sẵn sàng, Đang sử dụng, Đang sửa chữa)
/// </summary>
public class UsageStatus : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Color { get; private set; }
    public int? OrderNo { get; private set; }

    // Navigation properties
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();

    private UsageStatus()
    {
    }

    public static UsageStatus Create(string code, string name, string? description = null, string? color = null,
        int? orderNo = null)
    {
        return new UsageStatus
        {
            Code = code,
            Name = name,
            Description = description,
            Color = color,
            OrderNo = orderNo
        };
    }
}