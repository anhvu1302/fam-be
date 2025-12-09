using FAM.Domain.Common.Base;

namespace FAM.Domain.Assets.Events;

/// <summary>
/// Domain event: Audit/kiểm toán tài sản
/// </summary>
public sealed record AssetAudited : IDomainEvent
{
    public long AssetId { get; init; }
    public DateTime AuditDate { get; init; }
    public string? AuditResult { get; init; }
    public string? ComplianceStatus { get; init; }
    public int? AuditedBy { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}