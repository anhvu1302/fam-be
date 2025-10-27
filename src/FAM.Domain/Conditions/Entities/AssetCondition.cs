using FAM.Domain.Common;

namespace FAM.Domain.Conditions;

/// <summary>
/// Tình trạng tài sản (Tốt, Bình thường, Hỏng, v.v.)
/// </summary>
public class AssetCondition : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Navigation properties
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();

    private AssetCondition() { }

    public static AssetCondition Create(string name, string? description = null)
    {
        return new AssetCondition
        {
            Name = name,
            Description = description
        };
    }
}
