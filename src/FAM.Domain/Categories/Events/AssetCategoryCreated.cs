using FAM.Domain.Common.Base;

namespace FAM.Domain.Categories.Events;

public sealed record AssetCategoryCreated : IDomainEvent
{
    public int CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}