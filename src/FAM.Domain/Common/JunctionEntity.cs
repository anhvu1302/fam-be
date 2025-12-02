namespace FAM.Domain.Common;

/// <summary>
/// Base class for junction tables (many-to-many relationships)
/// Junction tables không cần Id riêng, chỉ dùng composite key
/// </summary>
public abstract class JunctionEntity
{
    // Chỉ cần timestamp đơn giản cho audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    protected JunctionEntity()
    {
    }
}