using FAM.Domain.Common;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Bảo hành/Bảo hiểm/License sắp hết hạn
/// </summary>
public sealed record AssetExpirationWarning : IDomainEvent
{
    public long AssetId { get; init; }
    public string ExpirationType { get; init; } = string.Empty; // Warranty, Insurance, License, Support
    public DateTime ExpirationDate { get; init; }
    public int DaysRemaining { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
